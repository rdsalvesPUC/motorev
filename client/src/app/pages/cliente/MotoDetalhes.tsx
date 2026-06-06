import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router';
import {
  Typography,
  Button,
  Flex,
  Card,
  Row,
  Col,
  Descriptions,
  Tag,
  Result,
  Skeleton,
  Modal,
  message,
  Popconfirm,
} from 'antd';
import {
  CarOutlined,
  CalendarOutlined,
  DashboardOutlined,
  ArrowLeftOutlined,
  EnvironmentOutlined,
  InfoCircleOutlined,
  DeleteOutlined,
} from '@ant-design/icons';
import { motoService } from "@/app/services/motoService";
import { Moto } from "@/app/models/Moto";
import { t } from '@/app/i18n';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { PATHS } from '@/app/paths';
import { getImageUrl } from '@/app/utils/imageUtils';
import { ApiError } from '@/app/services/http';

const { Title, Text } = Typography;

export default function MotoDetalhes() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [moto, setMoto] = useState<Moto | null>(null);
  const [loading, setLoading] = useState(true);
  const [errorStatus, setErrorStatus] = useState<number | null>(null);
  const [errorMessage, setErrorMessage] = useState<string>('');
  const [deleting, setDeleting] = useState(false);

  const executarRemocao = async () => {
    if (!moto) return;
    setDeleting(true);
    try {
      await motoService.delete(moto.id);
      message.success(t('motoDetalhes.remove.success'));
      navigate(PATHS.CLIENTE_MOTOS);
    } catch (err: any) {
      console.error('Falha ao inativar moto:', err);
      const isPendingAppointments = err instanceof ApiError && err.status === 422;
      Modal.error({
        title: t('motoDetalhes.remove.error.title'),
        content: isPendingAppointments
          ? t('motoDetalhes.remove.error.pendingAppointments')
          : (err.message || t('error.deleteMoto')),
      });
    } finally {
      setDeleting(false);
    }
  };



  useEffect(() => {
    const carregarDetalhes = async () => {
      if (!id) return;
      try {
        const data = await motoService.getById(Number(id));
        setMoto(data);
      } catch (err: any) {
        console.error('Falha ao carregar detalhes da moto:', err);
        if (err instanceof ApiError) {
          setErrorStatus(err.status ?? 500);
          setErrorMessage(err.message);
        } else {
          setErrorStatus(500);
          setErrorMessage(err.message || String(err));
        }
      } finally {
        setLoading(false);
      }
    };

    carregarDetalhes();
  }, [id]);

  if (loading) {
    return (
      <Flex vertical gap="large" style={{ width: '100%' }}>
        <DashboardBreadcrumb
          userType="cliente"
          items={[
            { title: t('minhasMotos.title'), path: PATHS.CLIENTE_MOTOS, icon: <CarOutlined /> },
            { title: t('motoDetalhes.title'), icon: <InfoCircleOutlined /> },
          ]}
        />
        <Card>
          <Skeleton active avatar paragraph={{ rows: 8 }} />
        </Card>
      </Flex>
    );
  }

  if (errorStatus) {
    return (
      <Flex vertical justify="center" align="center" style={{ width: '100%', minHeight: '60vh' }}>
        <Result
          status={'404'}
          title={ t('motoDetalhes.notFound.title')}
          subTitle={ t('motoDetalhes.notFound.message') }
          extra={
            <Button type="primary" icon={<ArrowLeftOutlined />} onClick={() => navigate(PATHS.CLIENTE_MOTOS)}>
              {t('motoDetalhes.back')}
            </Button>
          }
        />
      </Flex>
    );
  }

  if (!moto) return null;

  const dataVendaFormatada = new Date(moto.dataVenda).toLocaleDateString('pt-BR');

  return (
    <Flex vertical gap="large" style={{ width: '100%' }}>
      <Flex vertical gap="middle">
        <DashboardBreadcrumb
          userType="cliente"
          items={[
            { title: t('minhasMotos.title'), path: PATHS.CLIENTE_MOTOS, icon: <CarOutlined /> },
            { title: `${moto.marca} ${moto.nomeModelo}`, icon: <InfoCircleOutlined /> },
          ]}
        />

        <Flex justify="space-between" align="center" wrap="wrap" gap="middle">
          <Flex align="center" gap="middle">
            <Button
              icon={<ArrowLeftOutlined />}
              onClick={() => navigate(PATHS.CLIENTE_MOTOS)}
            >
              {t('motoForm.back')}
            </Button>
            <Title level={2} style={{ margin: 0 }}>
              {t('motoDetalhes.title')}
            </Title>
          </Flex>
          <Popconfirm
            title={t('motoDetalhes.remove.title')}
            description={t('motoDetalhes.remove.confirm', { nome: `${moto.marca} ${moto.nomeModelo} · ${moto.placa}` })}
            onConfirm={executarRemocao}
            okText={t('motoDetalhes.remove.ok')}
            cancelText={t('motoDetalhes.remove.cancel')}
            okButtonProps={{ danger: true, loading: deleting }}
            placement="bottomRight"
          >
            <Button
              danger
              type="primary"
              icon={<DeleteOutlined />}
              loading={deleting}
            >
              {t('motoDetalhes.remove')}
            </Button>
          </Popconfirm>
        </Flex>
      </Flex>

      <Row gutter={[24, 24]}>
        <Col xs={24} md={8}>
          <Card
            cover={
              <Flex
                justify="center"
                align="center"
                style={{
                  height: 280,
                  background: 'linear-gradient(135deg, #1f1c2c 0%, #928dab 100%)',
                }}
              >
                {moto.foto ? (
                  <img
                    src={getImageUrl(moto.foto)}
                    alt={`${moto.marca} ${moto.nomeModelo}`}
                    style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                  />
                ) : (
                  <CarOutlined style={{ fontSize: 80, color: 'rgba(255,255,255,0.4)' }} />
                )}
                <span style={{ position: 'absolute', top: 16, right: 16 }}>
                  <Tag color="blue" style={{ fontSize: '14px', padding: '4px 8px' }}>
                    {moto.cor}
                  </Tag>
                </span>
              </Flex>
            }
          >
            <Flex vertical gap="small" align="center">
              <Title level={3} style={{ margin: 0, textAlign: 'center' }}>
                {moto.marca} {moto.nomeModelo}
              </Title>
              <Text type="secondary" style={{ fontSize: '14px' }}>
                {moto.linha} · {moto.cilindrada}
              </Text>
              <Flex gap="small" style={{ marginTop: '8px' }}>
                <Tag color="purple">{moto.ano}</Tag>
                {moto.nomeConcessionaria && (
                  <Tag color="cyan" icon={<EnvironmentOutlined />}>
                    {moto.nomeConcessionaria}
                  </Tag>
                )}
              </Flex>
            </Flex>
          </Card>
        </Col>

        <Col xs={24} md={16}>
          <Flex vertical gap="large">
            <Card title={t('motoDetalhes.section.vehicle')} bordered={false}>
              <Descriptions column={{ xs: 1, sm: 2, md: 2, lg: 2, xl: 2, xxl: 2 }} bordered size="middle">
                <Descriptions.Item label={t('motoDetalhes.label.placa')}>
                  <Text strong style={{ letterSpacing: '1px' }}>{moto.placa}</Text>
                </Descriptions.Item>
                <Descriptions.Item label={t('motoDetalhes.label.chassi')}>
                  <Text strong>{moto.chassi}</Text>
                </Descriptions.Item>
                <Descriptions.Item label={t('motoDetalhes.label.cor')}>
                  {moto.cor}
                </Descriptions.Item>
                <Descriptions.Item label={t('motoDetalhes.label.kilometragem')}>
                  <Flex align="center" gap={6}>
                    <DashboardOutlined style={{ color: '#8c8c8c' }} />
                    <Text>{moto.kilometragemAtual.toLocaleString('pt-BR')} km</Text>
                  </Flex>
                </Descriptions.Item>
                <Descriptions.Item label={t('motoDetalhes.label.dataVenda')}>
                  <Flex align="center" gap={6}>
                    <CalendarOutlined style={{ color: '#8c8c8c' }} />
                    <Text>{dataVendaFormatada}</Text>
                  </Flex>
                </Descriptions.Item>
                {moto.nomeConcessionaria && (
                  <Descriptions.Item label={t('motoDetalhes.label.concessionaria')}>
                    {moto.nomeConcessionaria}
                  </Descriptions.Item>
                )}
              </Descriptions>
            </Card>

            <Card title={t('motoDetalhes.section.model')} bordered={false}>
              <Descriptions column={{ xs: 1, sm: 2, md: 2, lg: 2, xl: 2, xxl: 2 }} bordered size="middle">
                <Descriptions.Item label={t('motoDetalhes.label.modelo')}>
                  {moto.nomeModelo}
                </Descriptions.Item>
                <Descriptions.Item label={t('motoDetalhes.label.marca')}>
                  {moto.marca}
                </Descriptions.Item>
                <Descriptions.Item label={t('motoDetalhes.label.linha')}>
                  {moto.linha}
                </Descriptions.Item>
                <Descriptions.Item label={t('motoDetalhes.label.cilindrada')}>
                  {moto.cilindrada}
                </Descriptions.Item>
                <Descriptions.Item label={t('motoDetalhes.label.ano')}>
                  {moto.ano}
                </Descriptions.Item>
              </Descriptions>
            </Card>
          </Flex>
        </Col>
      </Row>
    </Flex>
  );
}
