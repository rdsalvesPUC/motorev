import {
  Typography,
  Button,
  Popconfirm,
  Space,
  Breadcrumb,
  Tag,
  Card,
  Row,
  Col,
  Empty,
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  HomeOutlined,
  CarOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { PATHS } from '../paths';

const { Title, Text } = Typography;

export interface Moto {
  key: string;
  marca: string;
  modelo: string;
  ano: number;
  placa: string;
  cor: string;
  kmAtual: number;
  status: 'ativa' | 'inativa';
  foto?: string;
}

const motos: Moto[] = [
  {
    key: '1',
    marca: 'Honda',
    modelo: 'CB 500F',
    ano: 2021,
    placa: 'ABC-1234',
    cor: 'Vermelho',
    kmAtual: 15000,
    status: 'ativa',
  },
  {
    key: '2',
    marca: 'Yamaha',
    modelo: 'MT-07',
    ano: 2020,
    placa: 'DEF-5678',
    cor: 'Azul',
    kmAtual: 32000,
    status: 'ativa',
  },
];

export default function MinhasMotos() {
  const navigate = useNavigate();

  return (
    <>
      <Breadcrumb
        style={{ marginBottom: 16 }}
        items={[
          {
            title: (
              <span style={{ cursor: 'pointer' }} onClick={() => navigate(PATHS.DASHBOARD_CLIENTE)}>
                <HomeOutlined style={{ marginRight: 4 }} />
                Início
              </span>
            ),
          },
          {
            title: (
              <span>
                <CarOutlined style={{ marginRight: 4 }} />
                Minhas Motos
              </span>
            ),
          },
        ]}
      />

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
        <Title level={2} style={{ margin: 0 }}>Minhas Motos</Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate(PATHS.CLIENTE_MOTOS_NOVA)}>
          Adicionar Moto
        </Button>
      </div>

      {motos.length === 0 ? (
        <Empty description="Nenhuma moto cadastrada" />
      ) : (
        <Row gutter={[24, 24]}>
          {motos.map((moto) => (
            <Col key={moto.key} xs={24} sm={12} lg={8} xl={6}>
              <Card
                cover={
                  moto.foto ? (
                    <img
                      src={moto.foto}
                      alt={`${moto.marca} ${moto.modelo}`}
                      style={{ height: 180, objectFit: 'cover' }}
                    />
                  ) : (
                    <div
                      style={{
                        height: 180,
                        display: 'flex',
                        flexDirection: 'column',
                        alignItems: 'center',
                        justifyContent: 'center',
                        background: '#fafafa',
                        borderBottom: '1px solid #f0f0f0',
                      }}
                    >
                      <CarOutlined style={{ fontSize: 48, color: '#d9d9d9' }} />
                      <Text type="secondary" style={{ marginTop: 8, fontSize: 12 }}>Sem foto</Text>
                    </div>
                  )
                }
                actions={[
                  <Button
                    key="edit"
                    type="text"
                    icon={<EditOutlined />}
                    onClick={() => navigate('/dashboard/cliente/motos/editar', { state: { moto } })}
                  >
                    Editar
                  </Button>,
                  <Popconfirm
                    key="delete"
                    title="Remover moto"
                    description="Tem certeza que deseja remover esta moto?"
                    okText="Sim"
                    cancelText="Não"
                  >
                    <Button type="text" icon={<DeleteOutlined />} danger>
                      Remover
                    </Button>
                  </Popconfirm>,
                ]}
              >
                <Card.Meta
                  title={
                    <Space>
                      <span>{moto.marca} {moto.modelo}</span>
                      <Tag color={moto.status === 'ativa' ? 'green' : 'default'}>
                        {moto.status === 'ativa' ? 'Ativa' : 'Inativa'}
                      </Tag>
                    </Space>
                  }
                  description={
                    <Space direction="vertical" size={2}>
                      <Text type="secondary">Ano: <Text>{moto.ano}</Text></Text>
                      <Text type="secondary">Placa: <Tag>{moto.placa}</Tag></Text>
                      <Text type="secondary">Cor: <Text>{moto.cor}</Text></Text>
                      <Text type="secondary">KM: <Text>{moto.kmAtual.toLocaleString('pt-BR')} km</Text></Text>
                    </Space>
                  }
                />
              </Card>
            </Col>
          ))}
        </Row>
      )}
    </>
  );
}
