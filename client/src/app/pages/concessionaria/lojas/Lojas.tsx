import { useEffect, useMemo, useState } from 'react';
import { Typography, Input, Button, Table, Space, Flex, Popconfirm, message, Spin, Tag, Empty, Switch } from 'antd';
import { ShopOutlined, SearchOutlined, PlusOutlined, EditOutlined, CompassOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { concessionariaService } from '@/app/services/concessionariaService';
import { Loja } from '@/app/models/Loja';
import { handleApiError } from '@/app/utils/errorHandler';

const { Title } = Typography;

interface LojaData extends Loja {
  key: string;
}

interface LojasProps {
  onNavigateToForm: () => void;
  onNavigateToEdit: (id: number) => void;
}

const isMatriz = (loja: LojaData) => loja.tipo.toLowerCase() === 'matriz';

export default function Lojas({ onNavigateToForm, onNavigateToEdit }: LojasProps) {
  const [data, setData] = useState<LojaData[]>([]);
  const [concessionariaId, setConcessionariaId] = useState<number>();
  const [loading, setLoading] = useState(true);
  const [busca, setBusca] = useState('');

  const fetchLojas = async () => {
    try {
      setLoading(true);
      const concessionaria = await concessionariaService.getMe();
      const lojas = await concessionariaService.getLojas(concessionaria.id);

      setConcessionariaId(concessionaria.id);
      setData(lojas.map((loja) => ({ ...loja, key: loja.id.toString() })));
    } catch (error) {
      handleApiError(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchLojas();
  }, []);

  const lojasFiltradas = useMemo(() => {
    const termo = busca.toLowerCase();
    if (!termo) return data;

    return data.filter((loja) =>
      loja.nome.toLowerCase().includes(termo) ||
      loja.cnpj.includes(busca) ||
      loja.logradouro.toLowerCase().includes(termo) ||
      loja.bairro.toLowerCase().includes(termo) ||
      loja.cidade.toLowerCase().includes(termo) ||
      loja.uf.toLowerCase().includes(termo)
    );
  }, [busca, data]);

  const handleStatusToggle = async (loja: LojaData) => {
    if (!concessionariaId) return;

    try {
      setLoading(true);
      await concessionariaService.alternarStatusLoja(concessionariaId, loja.id);
      await fetchLojas();
      message.success('Status da loja atualizado com sucesso!');
    } catch (error) {
      handleApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const columns: ColumnsType<LojaData> = [
    {
      title: 'Nome da Loja',
      dataIndex: 'nome',
      key: 'nome',
    },
    {
      title: 'Tipo',
      dataIndex: 'tipo',
      key: 'tipo',
      render: (tipo: string) => <Tag color={tipo.toLowerCase() === 'matriz' ? 'gold' : 'blue'}>{tipo}</Tag>,
    },
    {
      title: 'CNPJ',
      dataIndex: 'cnpj',
      key: 'cnpj',
    },
    {
      title: 'Endereco',
      key: 'endereco',
      render: (_: unknown, loja: LojaData) =>
        `${loja.logradouro}, ${loja.numero} - ${loja.bairro}, ${loja.cidade}/${loja.uf}`,
    },
    {
      title: 'CEP',
      dataIndex: 'cep',
      key: 'cep',
    },
    {
      title: 'Status',
      key: 'status',
      width: 160,
      align: 'center',
      render: (_: unknown, loja: LojaData) => {
        const statusAction = loja.ativo ? 'inativar' : 'ativar';

        return (
          <Popconfirm
            title={loja.ativo ? 'Inativar loja' : 'Ativar loja'}
            description={`Tem certeza que deseja ${statusAction} "${loja.nome}"?`}
            onConfirm={() => handleStatusToggle(loja)}
            okText="Sim"
            cancelText="Nao"
          >
            <Switch checked={loja.ativo} checkedChildren="Ativo" unCheckedChildren="Inativo" />
          </Popconfirm>
        );
      },
    },
    {
      title: 'Acoes',
      key: 'actions',
      width: 220,
      render: (_: unknown, loja: LojaData) => {
        const mapsUrl = `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(
          `${loja.logradouro}, ${loja.numero}, ${loja.bairro}, ${loja.cidade}, ${loja.uf}, CEP ${loja.cep}`
        )}`;

        return (
          <Space size="small">
            <Button type="link" icon={<CompassOutlined />} href={mapsUrl} target="_blank">
              Ir Para
            </Button>
            {!isMatriz(loja) && (
              <Button type="link" icon={<EditOutlined />} onClick={() => onNavigateToEdit(loja.id)}>
                Editar
              </Button>
            )}
          </Space>
        );
      },
    },
  ];

  return (
    <Spin spinning={loading}>
      <Space orientation="vertical" size="large" style={{ width: '100%' }}>
        <Space orientation="vertical" size="middle" style={{ width: '100%' }}>
          <DashboardBreadcrumb
            userType="concessionaria"
            items={[
              {
                title: 'Lojas',
                icon: <ShopOutlined />,
              },
            ]}
          />

          <Flex justify="space-between" align="center" wrap="wrap" gap="middle">
            <Title level={2} style={{ margin: 0 }}>
              Lojas
            </Title>
            <Button type="primary" icon={<PlusOutlined />} onClick={onNavigateToForm}>
              Adicionar Loja
            </Button>
          </Flex>

          <Input
            placeholder="Buscar por nome, CNPJ ou endereco..."
            prefix={<SearchOutlined />}
            value={busca}
            onChange={(event) => setBusca(event.target.value)}
            allowClear
          />
        </Space>

        <Space orientation="vertical" size="middle" style={{ width: '100%' }}>
          <span>{lojasFiltradas.length} loja(s) encontrada(s)</span>
          <Table
            dataSource={lojasFiltradas}
            columns={columns}
            locale={{
              emptyText: <Empty description="Nenhuma loja encontrada" />,
            }}
          />
        </Space>
      </Space>
    </Spin>
  );
}
