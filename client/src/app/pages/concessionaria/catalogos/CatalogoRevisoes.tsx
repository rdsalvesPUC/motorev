import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router';
import {
  Breadcrumb,
  Typography,
  Input,
  Select,
  Button,
  Table,
  Space,
  Flex,
  Modal,
  Switch,
  Spin,
  Card,
  message,
} from 'antd';
import {
  HomeOutlined,
  ToolOutlined,
  SearchOutlined,
  EyeOutlined,
  PlusOutlined,
  EditOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { revisaoPadraoService } from '@/app/services/revisaoPadraoService';
import { linhaService } from '@/app/services/linhaService';
import { modeloMotoService } from '@/app/services/modeloMotoService';
import { handleApiError } from '@/app/utils/errorHandler';
import { Linha } from '@/app/models/Linha';
import { ModeloMoto } from '@/app/models/ModeloMoto';
import { RevisaoPadraoListResponse } from '@/app/models/RevisaoPadrao';

const { Title } = Typography;

interface CatalogoModelosRevisaoProps {
  onNavigateToForm?: () => void;
  onNavigateToEdit?: (linhaId: number) => void;
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

const groupByLinha = (revisoes: RevisaoPadraoListResponse[], allModelos: ModeloMoto[]): RevisaoLinhaData[] => {
  const map = new Map<number, RevisaoPadraoListResponse[]>();

  revisoes.forEach((revisao) => {
    const current = map.get(revisao.linhaId) || [];
    current.push(revisao);
    map.set(revisao.linhaId, current);
  });

  return Array.from(map.entries()).map(([linhaId, items]) => {
    const sorted = [...items].sort((a, b) => a.ordem - b.ordem);
    const ordens = new Set(sorted.map((item) => item.ordem));
    const modelos = allModelos.filter((m) => m.linhaId === linhaId).map((m) => m.nomeModelo).sort();
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

export default function CatalogoModelosRevisao({ onNavigateToForm, onNavigateToEdit }: CatalogoModelosRevisaoProps) {
  const navigate = useNavigate();

  const [data, setData] = useState<RevisaoLinhaData[]>([]);
  const [linhas, setLinhas] = useState<Linha[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchText, setSearchText] = useState('');
  const [linhaFilter, setLinhaFilter] = useState<number | undefined>();
  const [statusFilter, setStatusFilter] = useState<'Ativo' | 'Inativo' | 'Todos'>('Todos');

  const fetchRevisoes = async () => {
    try {
      setLoading(true);
      const [revisoes, linhasData, modelosData] = await Promise.all([
        revisaoPadraoService.listar(),
        linhaService.getAll(false),
        modeloMotoService.getAll(),
      ]);

      setData(groupByLinha(revisoes, modelosData));
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
      title: 'Modelo de Revisão',
      key: 'modeloRevisao',
      render: (_, record) => <strong>Plano Padrão — {record.nomeLinha}</strong>,
    },
    {
      title: 'Linha de Moto',
      dataIndex: 'nomeLinha',
      key: 'nomeLinha',
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
      title: 'Status',
      key: 'status',
      width: 180,
      align: 'center',
      render: (_, record) => (
        <Switch
          checked={record.ativo}
          onChange={() => handleStatusToggle(record)}
          checkedChildren="Ativo"
          unCheckedChildren="Inativo"
        />
      ),
    },
    {
      title: 'Ações',
      key: 'actions',
      width: 180,
      align: 'center',
      render: (_, record) => (
        <Space>
          <Button
            type="link"
            icon={<EyeOutlined />}
            onClick={() => {
              navigate(`/dashboard/concessionaria/catalogos-revisoes/linha/${record.linhaId}`);
            }}
          >
            Detalhes
          </Button>
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => {
              if (onNavigateToEdit) {
                onNavigateToEdit(record.linhaId);
              }
            }}
          >
            Editar
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <Spin spinning={loading}>
      <Flex vertical gap="large" style={{ width: '100%' }}>
        <Flex vertical gap="middle" style={{ width: '100%' }}>
          <Breadcrumb
            items={[
              { href: '', title: <HomeOutlined /> },
              {
                title: (
                  <>
                    <ToolOutlined />
                    <span> Catálogos</span>
                  </>
                ),
              },
              { title: 'Modelos de Revisão' },
            ]}
          />

          <Title level={2} style={{ margin: 0 }}>
            Modelos de Revisão
          </Title>

          <Flex gap="middle" align="center" wrap="wrap">
            <Input
              placeholder="Buscar modelos..."
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
        </Flex>

        <Card>
          <Space direction="vertical" size="middle" style={{ width: '100%' }}>
            <Flex justify="space-between" align="center">
              <span>Total: {filteredData.length} modelos de revisão</span>
              <Button type="primary" icon={<PlusOutlined />} onClick={onNavigateToForm}>
                Adicionar Modelo de Revisão
              </Button>
            </Flex>

            <Table
              dataSource={filteredData}
              columns={columns}
              rowKey="key"
              bordered
            />
          </Space>
        </Card>
      </Flex>
    </Spin>
  );
}
