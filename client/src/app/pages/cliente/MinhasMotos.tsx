import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import {
  Typography,
  Input,
  Button,
  Flex,
  Card,
  Divider,
  Empty,
  Tag,
  Modal,
  Statistic,
  Skeleton,
} from 'antd';
import {
  CarOutlined,
  SearchOutlined,
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  DashboardOutlined,
  CalendarOutlined,
  TagOutlined,
} from '@ant-design/icons';
import {motoService} from "@/app/services/motoService";
import {Moto} from "@/app/models/Moto";
import { t } from '@/app/i18n';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { PATHS } from '@/app/paths';
import { getImageUrl } from '@/app/utils/imageUtils';

const { Title, Text } = Typography;

interface MinhasMotosProps {
}

const CORES_TAG: Record<string, string> = {
  'Vermelha': 'red',
  'Azul': 'blue',
  'Preta': 'black',
  'Branca': 'default',
  'Cinza': 'default',
  'Verde': 'green',
  'Amarela': 'gold',
  'Laranja': 'orange',
  'Rosa': 'pink',
  'Prata': 'default',
};

function MotoCard({
                    moto,
                    onEdit,
                    onDelete,
                  }: {
  moto: Moto;
  onEdit: () => void;
  onDelete: () => void;
}) {
  const handleDelete = () => {
    Modal.confirm({
      title: t('minhasMotos.remove.title'),
      content: t('minhasMotos.remove.confirm', { nome: `${moto.marca} ${moto.nomeModelo} · ${moto.placa}` }),
      okText: t('minhasMotos.remove.ok'),
      okType: 'danger',
      cancelText: t('minhasMotos.remove.cancel'),
      onOk: onDelete,
    });
  };

  const dataVendaFormatada = new Date(moto.dataVenda).toLocaleDateString('pt-BR');

  return (
      <Card
          hoverable
          style={{ width: '100%' }}
          cover={
            <Flex
                justify="center"
                align="center"
                style={{ height: 160, background: '#f0f2f5', position: 'relative', overflow: 'hidden' }}
            >
              {moto.foto ? (
                  <img
                      src={getImageUrl(moto.foto)}
                      alt={`${moto.marca} ${moto.nomeModelo}`}
                      style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                  />
              ) : (
                  <CarOutlined style={{ fontSize: 48, color: '#bfbfbf' }} />
              )}
              <div style={{ position: 'absolute', top: 10, right: 10 }}>
                <Tag color={CORES_TAG[moto.cor] ?? 'default'}>{moto.cor}</Tag>
              </div>
            </Flex>
          }
          actions={[
            <Button
                key="editar"
                type="link"
                icon={<EditOutlined />}
                onClick={onEdit}
            >
              {t('minhasMotos.card.edit')}
            </Button>,
            <Button
                key="deletar"
                type="link"
                danger
                icon={<DeleteOutlined />}
                onClick={handleDelete}
            >
              {t('minhasMotos.card.remove')}
            </Button>,
          ]}
      >
        <Flex vertical gap="small">
          <Flex vertical gap={2}>
            <Text strong style={{ fontSize: 16 }}>
              {moto.marca} {moto.nomeModelo}
            </Text>
            <Flex align="center" gap={6} wrap="wrap">
              <Tag>{moto.ano}</Tag>
              <Tag color="blue">{moto.linha}</Tag>
              <Tag color="purple">{moto.cilindrada}</Tag>
            </Flex>
          </Flex>

          <Divider style={{ margin: '8px 0' }} />
          <Flex gap="large" justify="space-between">
            <Flex vertical gap={2}>
              <Text type="secondary" style={{ fontSize: 11 }}>{t('minhasMotos.card.placa')}</Text>
              <Text strong style={{ fontSize: 14, letterSpacing: 1 }}>{moto.placa}</Text>
            </Flex>
            <Flex vertical gap={2} align="flex-end">
              <Text type="secondary" style={{ fontSize: 11 }}>{t('minhasMotos.card.hodometro')}</Text>
              <Flex align="center" gap={4}>
                <DashboardOutlined style={{ color: '#8c8c8c', fontSize: 12 }} />
                <Text strong style={{ fontSize: 14 }}>
                  {moto.kilometragemAtual.toLocaleString('pt-BR')} km
                </Text>
              </Flex>
            </Flex>
          </Flex>
          <Flex align="center" gap={6}>
            <CalendarOutlined style={{ color: '#8c8c8c', fontSize: 12 }} />
            <Text type="secondary" style={{ fontSize: 12 }}>{t('minhasMotos.card.purchased', { date: dataVendaFormatada })}</Text>
          </Flex>
          <Flex align="center" gap={6}>
            <TagOutlined style={{ color: '#8c8c8c', fontSize: 12 }} />
            <Text type="secondary" style={{ fontSize: 12 }}>{t('minhasMotos.card.chassi', { chassi: moto.chassi })}</Text>
          </Flex>
        </Flex>
      </Card>
  );
}

export default function MinhasMotos({ }: MinhasMotosProps) {
  const [motos, setMotos] = useState<Moto[]>([]);
  const [loading, setLoading] = useState(true);
  const [busca, setBusca] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    const carregarMotos = async () => {
      try {
        const data = await motoService.getAll();
        setMotos(data);
      } catch (error) {
        console.error('Falha ao carregar motos:', error);
      } finally {
        setLoading(false);
      }
    };
    carregarMotos();
  }, []);

  const motosFiltradas = motos.filter(
      (m) =>
          `${m.marca} ${m.nomeModelo}`.toLowerCase().includes(busca.toLowerCase()) ||
          m.placa.toLowerCase().includes(busca.toLowerCase()) ||
          m.cor.toLowerCase().includes(busca.toLowerCase()),
  );

  const handleDelete = async (id: number) => {
    // Nota: O endpoint de deleção ainda não foi solicitado nas tasks, 
    // mas o botão está na UI. Removendo localmente por enquanto.
    setMotos((prev) => prev.filter((m) => m.id !== id));
  };

  if (loading) {
    return (
        <Flex vertical gap="large" style={{ width: '100%' }}>
          <Skeleton active paragraph={{ rows: 2 }} />
          <Flex gap="large">
            <Skeleton.Button active style={{ width: 140, height: 80 }} />
            <Skeleton.Button active style={{ width: 180, height: 80 }} />
          </Flex>
          <Flex wrap="wrap" gap="large">
            {[1, 2, 3].map(i => (
                <Card key={i} style={{ width: 'calc(33.33% - 16px)', minWidth: 280 }}>
                  <Skeleton active avatar paragraph={{ rows: 4 }} />
                </Card>
            ))}
          </Flex>
        </Flex>
    );
  }

  return (
      <Flex vertical gap="large" style={{ width: '100%' }}>
        <Flex vertical gap="middle">
          <DashboardBreadcrumb
              userType="cliente"
              items={[
                { title: t('minhasMotos.title'), icon: <CarOutlined /> },
              ]}
          />

          <Flex justify="space-between" align="center" wrap="wrap" gap="middle">
            <Title level={2} style={{ margin: 0 }}>
              {t('minhasMotos.title')}
            </Title>
            <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => navigate(PATHS.CLIENTE_MOTOS_NOVA)}
            >
              {t('minhasMotos.addMoto')}
            </Button>
          </Flex>

          <Input
              placeholder={t('minhasMotos.search.placeholder')}
              prefix={<SearchOutlined />}
              value={busca}
              onChange={(e) => setBusca(e.target.value)}
              allowClear
              style={{ maxWidth: 380 }}
          />
        </Flex>

        {motos.length > 0 && (
            <Flex gap="large" wrap="wrap">
              <Card size="small" style={{ minWidth: 140 }}>
                <Statistic title={t('minhasMotos.totalMotos')} value={motos.length} suffix={t('minhasMotos.totalMotos.suffix')} valueStyle={{ fontSize: 20 }} />
              </Card>
              <Card size="small" style={{ minWidth: 180 }}>
                <Statistic
                    title={t('minhasMotos.totalKm')}
                    value={motos.reduce((acc, m) => acc + m.kilometragemAtual, 0).toLocaleString('pt-BR')}
                    suffix={t('minhasMotos.totalKm.suffix')}
                    valueStyle={{ fontSize: 20 }}
                />
              </Card>
            </Flex>
        )}

        <Text type="secondary">
          {t('minhasMotos.found', { count: motosFiltradas.length })}
        </Text>

        {motosFiltradas.length === 0 ? (
            <Card>
              <Empty
                  description={
                    motos.length === 0
                        ? t('minhasMotos.empty')
                        : t('minhasMotos.notFound')
                  }
              >
                {motos.length === 0 && (
                    <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate(PATHS.CLIENTE_MOTOS_NOVA)}>
                      {t('minhasMotos.addFirstMoto')}
                    </Button>
                )}
              </Empty>
            </Card>
        ) : (
            <Flex wrap="wrap" gap="large">
              {motosFiltradas.map((moto) => (
                  <div key={moto.id} style={{ width: 'calc(33.33% - 16px)', minWidth: 280 }}>
                    <MotoCard
                        moto={moto}
                        onEdit={() => navigate(`${PATHS.CLIENTE_MOTOS_EDITAR}/${moto.id}`)}
                        onDelete={() => handleDelete(moto.id)}
                    />
                  </div>
              ))}
            </Flex>
        )}
      </Flex>
  );
}
