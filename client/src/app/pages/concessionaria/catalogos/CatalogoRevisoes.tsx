import { Breadcrumb, Typography, Input, Select, Button, Table, Space, Flex, Modal, Tag, App, Switch } from 'antd';
import { HomeOutlined, ToolOutlined, SearchOutlined, EditOutlined, EyeOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useModelosRevisao } from '../../contexts/ModelosRevisaoContext';
import type { ModeloRevisaoData } from '../../contexts/ModelosRevisaoContext';

const { Title } = Typography;

interface CatalogoModelosRevisaoProps {
  onNavigate: (page: string, modeloKey?: string) => void;
}

export default function CatalogoModelosRevisao({ onNavigate }: CatalogoModelosRevisaoProps) {
  const { modelosRevisao: data, setModelosRevisao: setData } = useModelosRevisao();
  const { modal } = App.useApp();

  const handleStatusToggle = (record: ModeloRevisaoData) => {
    const novoStatus = record.status === 'ativo' ? 'inativo' : (record.status === 'inativo' ? 'ativo' : 'ativo');

    if (novoStatus === 'inativo') {
      modal.confirm({
        title: 'Desativar modelo de revisão',
        content: 'Tem certeza que deseja desativar este modelo de revisão?',
        okText: 'Sim',
        cancelText: 'Não',
        onOk() {
          const newData = data.map(item =>
            item.key === record.key ? { ...item, status: novoStatus } : item
          );
          setData(newData);
        },
      });
    } else {
      const newData = data.map(item =>
        item.key === record.key ? { ...item, status: novoStatus } : item
      );
      setData(newData);
    }
  };

  const columns: ColumnsType<ModeloRevisaoData> = [
    {
      title: 'Modelo de Revisão',
      dataIndex: 'nome',
      key: 'nome',
    },
    {
      title: 'Linha de Moto',
      dataIndex: 'linha',
      key: 'linha',
      width: 150,
    },
    {
      title: 'Qtd. Revisões',
      dataIndex: 'quantidadeRevisoes',
      key: 'quantidadeRevisoes',
      width: 120,
      align: 'center',
    },
    {
      title: 'Modelos Vinculados',
      dataIndex: 'modelosVinculados',
      key: 'modelosVinculados',
      render: (modelos: string[]) => `${modelos.length} modelo(s)`,
      width: 150,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 120,
      align: 'center',
      render: (_: any, record: ModeloRevisaoData) => {
        if (record.status === 'rascunho') {
          return <Tag color="blue">Rascunho</Tag>;
        }
        return (
          <Switch
            checked={record.status === 'ativo'}
            onChange={() => handleStatusToggle(record)}
            checkedChildren="Ativo"
            unCheckedChildren="Inativo"
          />
        );
      },
    },
    {
      title: 'Ações',
      key: 'actions',
      width: 1,
      render: (_: any, record: ModeloRevisaoData) => (
        <Space>
          <Button
            key="detalhes"
            type="link"
            icon={<EyeOutlined />}
            onClick={() => onNavigate('modelo-revisao-detalhes', record.key)}
          >
            Detalhes
          </Button>
          <Button
            key="edit"
            type="link"
            icon={<EditOutlined />}
            onClick={() => onNavigate('catalogos-modelos-revisao-edit', record.key)}
          >
            Editar
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <Flex vertical gap="large" style={{ width: '100%' }}>
      <Flex vertical gap="middle" style={{ width: '100%' }}>
        <Breadcrumb
          items={[
            {
              href: '',
              title: <HomeOutlined />,
            },
            {
              title: (
                <>
                  <ToolOutlined />
                  <span>Catálogos</span>
                </>
              ),
            },
            {
              title: 'Modelos de Revisão',
            },
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
          />

          <Select
            placeholder="Linha"
            style={{ width: 150 }}
            options={[
              { value: 'passeio', label: 'Passeio' },
              { value: 'trail', label: 'Trail' },
              { value: 'esportiva', label: 'Esportiva' },
              { value: 'scooter', label: 'Scooter' },
            ]}
          />

          <Select
            placeholder="Status"
            style={{ width: 120 }}
            options={[
              { value: 'ativo', label: 'Ativo' },
              { value: 'rascunho', label: 'Rascunho' },
              { value: 'inativo', label: 'Inativo' },
            ]}
          />

          <Flex gap="small" style={{ marginLeft: 'auto' }}>
            <Button>Limpar</Button>
            <Button type="primary">Aplicar</Button>
          </Flex>
        </Flex>
      </Flex>

      <div style={{ background: '#fff', padding: '24px', borderRadius: '8px' }}>
        <Flex vertical gap="middle" style={{ width: '100%' }}>
          <Flex justify="space-between" align="center">
            <span>Total: {data.length} modelos de revisão</span>
            <Button type="primary" onClick={() => onNavigate('catalogos-modelos-revisao-create')}>
              Adicionar Modelo de Revisão
            </Button>
          </Flex>

          <Table
            bordered
            dataSource={data}
            columns={columns}
          />
        </Flex>
      </div>
    </Flex>
  );
}
