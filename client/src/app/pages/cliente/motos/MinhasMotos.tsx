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
  message,
  Popconfirm,
  theme,
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
import { getLocale, t } from '@/app/i18n';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { PATHS } from '@/app/paths';
import { getImageUrl } from '@/app/utils/imageUtils';
import { ApiError } from '@/app/services/http';

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

const COR_LABEL_KEY: Record<string, string> = {
  'Preta': 'motoForm.color.black',
  'Branca': 'motoForm.color.white',
  'Vermelha': 'motoForm.color.red',
  'Azul': 'motoForm.color.blue',
  'Cinza': 'motoForm.color.gray',
  'Prata': 'motoForm.color.silver',
  'Verde': 'motoForm.color.green',
  'Amarela': 'motoForm.color.yellow',
  'Laranja': 'motoForm.color.orange',
  'Rosa': 'motoForm.color.pink',
  'Outra': 'motoForm.color.other',
};

function MotoCard({
                    moto,
                    onViewDetails,
                    onEdit,
                    onDelete,
                  }: {
  moto: Moto;
  onViewDetails: () => void;
  onEdit: () => void;
  onDelete: () => void;
}) {
  const { token } = theme.useToken();
  const dataVendaFormatada = new Date(moto.dataVenda).toLocaleDateString(getLocale());
  const corLabel = COR_LABEL_KEY[moto.cor] ? t(COR_LABEL_KEY[moto.cor]) : moto.cor;

  return (
      <Card
          hoverable
          style={{ width: '100%' }}
          onClick={onViewDetails}
          cover={
            <Flex
                justify="center"
                align="center"
                style={{ height: 160, background: token.colorFillSecondary, position: 'relative', overflow: 'hidden' }}
            >
              {moto.foto ? (
                  <img
                      src={getImageUrl(moto.foto)}
                      alt={`${moto.marca} ${moto.nomeModelo}`}
                      style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                  />
              ) : (
                  <CarOutlined style={{ fontSize: 48, color: token.colorTextQuaternary }} />
              )}
              <span style={{ position: 'absolute', top: 10, right: 10 }}>
                <Tag color={CORES_TAG[moto.cor] ?? 'default'}>{corLabel}</Tag>
              </span>
            </Flex>
          }
          actions={[
            <Button
                key="editar"
                type="link"
                icon={<EditOutlined />}
                onClick={(e) => {
                  e.stopPropagation();
                  onEdit();
                }}
            >
              {t('minhasMotos.card.edit')}
            </Button>,
            <Popconfirm
                key="deletar"
                title={t('minhasMotos.remove.title')}
                description={t('minhasMotos.remove.confirm', { nome: `${moto.marca} ${moto.nomeModelo} · ${moto.placa}` })}
                onConfirm={(e) => {
                  e?.stopPropagation();
                  onDelete();
                }}
                onCancel={(e) => e?.stopPropagation()}
                okText={t('minhasMotos.remove.ok')}
                cancelText={t('minhasMotos.remove.cancel')}
                okButtonProps={{ danger: true }}
            >
              <Button
                  type="link"
                  danger
                  icon={<DeleteOutlined />}
                  onClick={(e) => e.stopPropagation()}
              >
                {t('minhasMotos.card.remove')}
              </Button>
            </Popconfirm>,
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
                <DashboardOutlined style={{ color: token.colorTextTertiary, fontSize: 12 }} />
                <Text strong style={{ fontSize: 14 }}>
                  {moto.kilometragemAtual.toLocaleString(getLocale())} km
                </Text>
              </Flex>
            </Flex>
          </Flex>
          <Flex align="center" gap={6}>
            <CalendarOutlined style={{ color: token.colorTextTertiary, fontSize: 12 }} />
            <Text type="secondary" style={{ fontSize: 12 }}>{t('minhasMotos.card.purchased', { date: dataVendaFormatada })}</Text>
          </Flex>
          <Flex align="center" gap={6}>
            <TagOutlined style={{ color: token.colorTextTertiary, fontSize: 12 }} />
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
    try {
      await motoService.delete(id);
      message.success(t('motoDetalhes.remove.success'));
      setMotos((prev) => prev.filter((m) => m.id !== id));
    } catch (err: any) {
      console.error('Falha ao inativar moto:', err);
      const isPendingAppointments = err instanceof ApiError && err.status === 422;
      Modal.error({
        title: t('motoDetalhes.remove.error.title'),
        content: isPendingAppointments
          ? t('motoDetalhes.remove.error.pendingAppointments')
          : (err.message || t('error.deleteMoto')),
      });
    }
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
                    value={motos.reduce((acc, m) => acc + m.kilometragemAtual, 0).toLocaleString(getLocale())}
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
                  <article key={moto.id} style={{ width: 'calc(33.33% - 16px)', minWidth: 280 }}>
                    <MotoCard
                        moto={moto}
                        onViewDetails={() => navigate(`${PATHS.CLIENTE_MOTOS_DETALHES}/${moto.id}`)}
                        onEdit={() => navigate(`${PATHS.CLIENTE_MOTOS_EDITAR}/${moto.id}`)}
                        onDelete={() => handleDelete(moto.id)}
                    />
                  </article>
              ))}
            </Flex>
        )}
      </Flex>
  );
}
