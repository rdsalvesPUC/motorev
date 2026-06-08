import { useEffect, useMemo, useState } from 'react';
import { Typography, Input, Select, Button, Table, Space, Flex, Popconfirm, message, Spin, Tag, Card, Empty, Switch } from 'antd';
import { ToolOutlined, SearchOutlined, PlusOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { revisaoPadraoService } from '@/app/services/revisaoPadraoService';
import { linhaService } from '@/app/services/linhaService';
import { handleApiError } from '@/app/utils/errorHandler';
import { Linha } from '@/app/models/Linha';
import { RevisaoPadraoListResponse } from '@/app/models/RevisaoPadrao';

const { Title } = Typography;

interface CatalogoModelosRevisaoProps {
  onNavigateToForm?: () => void;
}

interface RevisaoLinhaData {
  key: string;
  linhaId: number;
  nomeLinha: string;
  quantidadeRevisoes: number;
  modelosVinculados: string[];
  quilometragens: number[];
  tempoMeses: number[];
  ativo: boolean;
  revisoes: RevisaoPadraoListResponse[];
}

const groupByLinha = (revisoes: RevisaoPadraoListResponse[]): RevisaoLinhaData[] => {
  const map = new Map<number, RevisaoPadraoListResponse[]>();

  revisoes.forEach((revisao) => {
    const current = map.get(revisao.linhaId) || [];
    current.push(revisao);
    map.set(revisao.linhaId, current);
  });

  return Array.from(map.entries()).map(([linhaId, items]) => {
    const sorted = [...items].sort((a, b) => a.ordem - b.ordem || a.nomeModeloMoto.localeCompare(b.nomeModeloMoto));
    const ordens = new Set(sorted.map((item) => item.ordem));
    const modelos = Array.from(new Set(sorted.map((item) => item.nomeModeloMoto))).sort();
    const quilometragens = Array.from(new Set(sorted.map((item) => item.quilometragem))).sort((a, b) => a - b);
    const tempoMeses = Array.from(new Set(sorted.map((item) => item.tempoMeses))).sort((a, b) => a - b);

    return {
      key: linhaId.toString(),
      linhaId,
      nomeLinha: sorted[0]?.nomeLinha || `Linha ${linhaId}`,
      quantidadeRevisoes: ordens.size,
      modelosVinculados: modelos,
      quilometragens,
      tempoMeses,
      ativo: sorted.some((item) => item.ativo),
      revisoes: sorted,
    };
  }).sort((a, b) => a.nomeLinha.localeCompare(b.nomeLinha));
};

const formatRange = (values: number[], suffix: string) => {
  if (values.length === 0) return '-';
  if (values.length === 1) return `${values[0].toLocaleString('pt-BR')} ${suffix}`;

  return `${values[0].toLocaleString('pt-BR')} - ${values[values.length - 1].toLocaleString('pt-BR')} ${suffix}`;
};

export default function CatalogoModelosRevisao({ onNavigateToForm }: CatalogoModelosRevisaoProps) {
  const [data, setData] = useState<RevisaoLinhaData[]>([]);
  const [linhas, setLinhas] = useState<Linha[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchText, setSearchText] = useState('');
  const [linhaFilter, setLinhaFilter] = useState<number | undefined>();
  const [statusFilter, setStatusFilter] = useState<'Ativo' | 'Inativo' | 'Todos'>('Todos');

  const fetchRevisoes = async () => {
    try {
      setLoading(true);
      const [revisoes, linhasData] = await Promise.all([
        revisaoPadraoService.listar(),
        linhaService.getAll(false),
      ]);

      setData(groupByLinha(revisoes));
      setLinhas(linhasData);
    } catch (error) {
      handleApiError(error, 'Erro ao carregar modelos de revisão.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRevisoes();
  }, []);

  const filteredData = useMemo(() => {
    const search = searchText.toLowerCase();

    return data.filter((item) => {
      const matchesSearch = !searchText
        || item.nomeLinha.toLowerCase().includes(search)
        || item.modelosVinculados.some((modelo) => modelo.toLowerCase().includes(search));
      const matchesLinha = !linhaFilter || item.linhaId === linhaFilter;
      const matchesStatus = statusFilter === 'Todos'
        || (statusFilter === 'Ativo' && item.ativo)
        || (statusFilter === 'Inativo' && !item.ativo);

      return matchesSearch && matchesLinha && matchesStatus;
    });
  }, [data, linhaFilter, searchText, statusFilter]);

  const handleStatusToggle = async (record: RevisaoLinhaData) => {
    try {
      setLoading(true);
      await revisaoPadraoService.alternarStatusPorLinha(record.linhaId);
      await fetchRevisoes();
      message.success('Status das revisões atualizado com sucesso.');
    } catch (error) {
      handleApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const handleClearFilters = () => {
    setSearchText('');
    setLinhaFilter(undefined);
    setStatusFilter('Todos');
  };

  const columns: ColumnsType<RevisaoLinhaData> = [
    {
      title: 'Linha de Moto',
      dataIndex: 'nomeLinha',
      key: 'nomeLinha',
      render: (nomeLinha: string) => <strong>{nomeLinha}</strong>,
    },
    {
      title: 'Qtd. Revisões',
      dataIndex: 'quantidadeRevisoes',
      key: 'quantidadeRevisoes',
      width: 130,
      align: 'center',
    },
    {
      title: 'Modelos Vinculados',
      dataIndex: 'modelosVinculados',
      key: 'modelosVinculados',
      width: 180,
      render: (modelos: string[]) => `${modelos.length} modelo(s)`,
    },
    {
      title: 'Quilometragem',
      dataIndex: 'quilometragens',
      key: 'quilometragens',
      width: 180,
      render: (values: number[]) => formatRange(values, 'km'),
    },
    {
      title: 'Tempo',
      dataIndex: 'tempoMeses',
      key: 'tempoMeses',
      width: 160,
      render: (values: number[]) => formatRange(values, 'meses'),
    },
    {
      title: 'Status',
      key: 'status',
      width: 180,
      align: 'center',
      render: (_: any, record) => (
        <Space size="small">
          <Tag color={record.ativo ? 'green' : 'red'}>
            {record.ativo ? 'Ativo' : 'Inativo'}
          </Tag>
          <Popconfirm
            title={record.ativo ? 'Inativar revisões da linha' : 'Ativar revisões da linha'}
            description={record.ativo ? 'Tem certeza que deseja inativar as revisões desta linha?' : 'Tem certeza que deseja ativar as revisões desta linha?'}
            okText="Sim"
            cancelText="Não"
            onConfirm={() => handleStatusToggle(record)}
          >
            <Switch checked={record.ativo} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <Spin spinning={loading}>
      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
          <DashboardBreadcrumb
            userType="concessionaria"
            items={[
              {
                title: 'Modelos de Revisão',
                icon: <ToolOutlined />,
              },
            ]}
          />

          <Title level={2} style={{ margin: 0 }}>
            Modelos de Revisão
          </Title>

          <Flex gap="middle" align="center" wrap="wrap">
            <Input
              placeholder="Buscar por linha ou modelo..."
              prefix={<SearchOutlined />}
              style={{ width: 300 }}
              value={searchText}
              onChange={(event) => setSearchText(event.target.value)}
            />

            <Select
              placeholder="Linha"
              style={{ width: 180 }}
              value={linhaFilter}
              onChange={setLinhaFilter}
              allowClear
              options={linhas.map((linha) => ({ value: linha.id, label: linha.nome }))}
            />

            <Select
              placeholder="Status"
              style={{ width: 140 }}
              value={statusFilter}
              onChange={setStatusFilter}
              options={[
                { value: 'Ativo', label: 'Ativo' },
                { value: 'Inativo', label: 'Inativo' },
                { value: 'Todos', label: 'Todos' },
              ]}
            />

            <Flex gap="small" style={{ marginLeft: 'auto' }}>
              <Button onClick={handleClearFilters}>Limpar</Button>
              <Button type="primary" onClick={fetchRevisoes}>Aplicar</Button>
            </Flex>
          </Flex>
        </Space>

        <Card>
          <Space direction="vertical" size="middle" style={{ width: '100%' }}>
            <Flex justify="space-between" align="center">
              <span>Total: {filteredData.length} linha(s) com revisões</span>
              <Button type="primary" icon={<PlusOutlined />} onClick={onNavigateToForm}>
                Adicionar Modelo de Revisão
              </Button>
            </Flex>

            <Table
              dataSource={filteredData}
              columns={columns}
              rowKey="key"
              expandable={{
                expandedRowRender: (record) => (
                  <Table
                    size="small"
                    rowKey={(item) => `${item.modeloMotoId}-${item.ordem}`}
                    dataSource={record.revisoes}
                    pagination={false}
                    columns={[
                      { title: 'Ordem', dataIndex: 'ordem', key: 'ordem', width: 90 },
                      { title: 'Revisão', dataIndex: 'nome', key: 'nome' },
                      { title: 'Modelo', dataIndex: 'nomeModeloMoto', key: 'nomeModeloMoto' },
                      {
                        title: 'KM',
                        dataIndex: 'quilometragem',
                        key: 'quilometragem',
                        render: (value: number) => `${value.toLocaleString('pt-BR')} km`,
                      },
                      {
                        title: 'Tempo',
                        dataIndex: 'tempoMeses',
                        key: 'tempoMeses',
                        render: (value: number) => `${value} meses`,
                      },
                      {
                        title: 'Status',
                        dataIndex: 'ativo',
                        key: 'ativo',
                        render: (ativo: boolean) => (
                          <Tag color={ativo ? 'green' : 'red'}>{ativo ? 'Ativo' : 'Inativo'}</Tag>
                        ),
                      },
                    ]}
                  />
                ),
              }}
              locale={{
                emptyText: (
                  <Empty
                    image={Empty.PRESENTED_IMAGE_SIMPLE}
                    description="Nenhum modelo de revisão encontrado."
                  />
                ),
              }}
            />
          </Space>
        </Card>
      </Space>
    </Spin>
  );
}
