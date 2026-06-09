import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router';
import {
  Typography,
  Button,
  Flex,
  Card,
  Tag,
  Result,
  Skeleton,
  Modal,
  message,
  Popconfirm,
  Alert,
  Badge,
  Descriptions,
  Divider,
  Statistic,
  Table,
  Timeline,
  theme,
  Empty,
} from 'antd';
import {
  CarOutlined,
  CalendarOutlined,
  DashboardOutlined,
  ArrowLeftOutlined,
  DeleteOutlined,
  CheckCircleFilled,
  ClockCircleFilled,
  CloseCircleFilled,
  HourglassOutlined,
  ToolOutlined,
  TagOutlined,
  WarningFilled,
} from '@ant-design/icons';
import { motoService } from '@/app/services/motoService';
import { Moto, RevisaoMotoResponse } from '@/app/models/Moto';
import { getLocale, t } from '@/app/i18n';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { PATHS } from '@/app/paths';
import { getImageUrl } from '@/app/utils/imageUtils';
import { ApiError } from '@/app/services/http';
import { formatCurrency } from '@/app/utils/formatters';

const { Title, Text } = Typography;

type RevisionStatus = 'concluida' | 'em_execucao' | 'agendada' | 'atrasada' | 'planejada';
type ThemeToken = ReturnType<typeof theme.useToken>['token'];

const normalizeStatus = (status: string) =>
  status
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .replace(/\s+/g, '_');

const parseLocalDate = (date: string) => new Date(`${date.substring(0, 10)}T00:00:00`);

const formatDate = (date: string) => parseLocalDate(date).toLocaleDateString(getLocale());

const formatNumber = (value: number) => value.toLocaleString(getLocale());

const getRevisionStatus = (revisao: RevisaoMotoResponse): RevisionStatus => {
  const status = normalizeStatus(revisao.status);
  if (status.includes('concluida')) return 'concluida';
  if (status.includes('execucao')) return 'em_execucao';
  if (status.includes('agendada')) return 'agendada';

  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return parseLocalDate(revisao.dataPrevista) < today ? 'atrasada' : 'planejada';
};

const getRevisionEstimate = (revisao: RevisaoMotoResponse) => {
  const servicos = revisao.servicos?.reduce((total, servico) => total + (servico.custo || 0), 0) ?? 0;
  const pecas = revisao.pecas?.reduce((total, peca) => total + ((peca.preco || 0) * (peca.quantidade || 0)), 0) ?? 0;
  return servicos + pecas;
};

const getRevisionPartsEstimate = (revisao: RevisaoMotoResponse) =>
  revisao.pecas?.reduce((total, peca) => total + ((peca.preco || 0) * (peca.quantidade || 0)), 0) ?? 0;

const getRevisionServicesEstimate = (revisao: RevisaoMotoResponse) =>
  revisao.servicos?.reduce((total, servico) => total + (servico.custo || 0), 0) ?? 0;

const getRevisionTime = (revisao: RevisaoMotoResponse) =>
  revisao.servicos?.reduce((total, servico) => total + (servico.tempoEstimado || 0), 0) ?? 0;

const formatDuration = (minutes: number) => {
  if (!minutes) return '-';
  const hours = Math.floor(minutes / 60);
  const remainingMinutes = minutes % 60;
  if (!hours) return t('motoRevisaoDetalhes.duration.minutes', { minutes: remainingMinutes });
  if (!remainingMinutes) return t('motoRevisaoDetalhes.duration.hours', { hours });
  return t('motoRevisaoDetalhes.duration.hoursMinutes', { hours, minutes: remainingMinutes });
};

const getRevisionTitle = (revisao: RevisaoMotoResponse) =>
  t('motoDetalhes.revisions.order', { order: revisao.ordem });

const getDaysUntilRevision = (revisao: RevisaoMotoResponse) => {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return Math.ceil((parseLocalDate(revisao.dataPrevista).getTime() - today.getTime()) / 86400000);
};

const getStatusConfig = (status: RevisionStatus, token: ThemeToken) => {
  const configs = {
    concluida: {
      label: t('motoDetalhes.status.concluida'),
      badgeStatus: 'success' as const,
      tagColor: 'success',
      icon: <CheckCircleFilled style={{ color: token.colorSuccess }} />,
    },
    em_execucao: {
      label: t('motoDetalhes.status.emExecucao'),
      badgeStatus: 'processing' as const,
      tagColor: 'processing',
      icon: <ClockCircleFilled style={{ color: token.colorInfo }} />,
    },
    agendada: {
      label: t('motoDetalhes.status.agendada'),
      badgeStatus: 'processing' as const,
      tagColor: 'blue',
      icon: <CalendarOutlined style={{ color: token.colorInfo }} />,
    },
    atrasada: {
      label: t('motoDetalhes.status.atrasada'),
      badgeStatus: 'error' as const,
      tagColor: 'error',
      icon: <CloseCircleFilled style={{ color: token.colorError }} />,
    },
    planejada: {
      label: t('motoDetalhes.status.planejada'),
      badgeStatus: 'default' as const,
      tagColor: 'default',
      icon: <HourglassOutlined style={{ color: token.colorTextTertiary }} />,
    },
  };

  return configs[status];
};

function RevisaoPlanejadaCard({ revisao, onClick }: { revisao: RevisaoMotoResponse; onClick: () => void }) {
  const { token } = theme.useToken();
  const status = getRevisionStatus(revisao);
  const statusConfig = getStatusConfig(status, token);
  const estimate = getRevisionEstimate(revisao);

  return (
    <Card
      hoverable
      size="small"
      onClick={onClick}
      style={{ borderColor: status === 'atrasada' ? token.colorErrorBorder : undefined }}
      styles={{ body: { padding: 16 } }}
    >
      <Flex justify="space-between" align="flex-start" gap="middle" wrap="wrap">
        <Flex vertical gap={8} style={{ flex: 1, minWidth: 220 }}>
          <Flex align="center" gap={8} wrap="wrap">
            <Text strong>{revisao.nome}</Text>
            <Badge status={statusConfig.badgeStatus} text={statusConfig.label} />
          </Flex>

          <Flex gap="large" wrap="wrap">
            <Flex align="center" gap={6}>
              <DashboardOutlined style={{ color: token.colorTextTertiary, fontSize: 12 }} />
              <Text type="secondary" style={{ fontSize: 12 }}>
                {formatNumber(revisao.quilometragem)} km
              </Text>
            </Flex>
            <Flex align="center" gap={6}>
              <CalendarOutlined style={{ color: token.colorTextTertiary, fontSize: 12 }} />
              <Text type="secondary" style={{ fontSize: 12 }}>
                {t('motoDetalhes.revisions.idealDate', { date: formatDate(revisao.dataPrevista) })}
              </Text>
            </Flex>
            <Flex align="center" gap={6}>
              <ToolOutlined style={{ color: token.colorTextTertiary, fontSize: 12 }} />
              <Text type="secondary" style={{ fontSize: 12 }}>
                {t('motoDetalhes.revisions.items', {
                  parts: revisao.pecas?.length ?? 0,
                  services: revisao.servicos?.length ?? 0,
                })}
              </Text>
            </Flex>
          </Flex>

          <Text type="secondary" style={{ fontSize: 12 }}>
            {t('motoDetalhes.revisions.estimate', { value: formatCurrency(estimate) })}
          </Text>
        </Flex>

        <Tag color={statusConfig.tagColor}>{getRevisionTitle(revisao)}</Tag>
      </Flex>
    </Card>
  );
}

function RevisaoDetalhes({
  moto,
  revisao,
  onBack,
}: {
  moto: Moto;
  revisao: RevisaoMotoResponse;
  onBack: () => void;
}) {
  const { token } = theme.useToken();
  const status = getRevisionStatus(revisao);
  const statusConfig = getStatusConfig(status, token);
  const daysUntilRevision = getDaysUntilRevision(revisao);
  const totalPecas = getRevisionPartsEstimate(revisao);
  const totalServicos = getRevisionServicesEstimate(revisao);
  const totalGeral = totalPecas + totalServicos;
  const totalTempo = getRevisionTime(revisao);

  const pecasColumns = [
    { title: t('motoRevisaoDetalhes.parts.code'), dataIndex: 'codigo', key: 'codigo', width: 120 },
    { title: t('motoRevisaoDetalhes.parts.name'), dataIndex: 'nome', key: 'nome' },
    { title: t('motoRevisaoDetalhes.parts.quantity'), dataIndex: 'quantidade', key: 'quantidade', width: 90, align: 'center' as const },
    {
      title: t('motoRevisaoDetalhes.parts.unitValue'),
      dataIndex: 'preco',
      key: 'preco',
      width: 130,
      align: 'right' as const,
      render: (value: number) => formatCurrency(value),
    },
    {
      title: t('motoRevisaoDetalhes.parts.total'),
      key: 'total',
      width: 130,
      align: 'right' as const,
      render: (_: unknown, record: RevisaoMotoResponse['pecas'][number]) => (
        <Text strong>{formatCurrency((record.preco || 0) * (record.quantidade || 0))}</Text>
      ),
    },
  ];

  const servicosColumns = [
    { title: t('motoRevisaoDetalhes.services.name'), dataIndex: 'nome', key: 'nome' },
    {
      title: t('motoRevisaoDetalhes.services.time'),
      dataIndex: 'tempoEstimado',
      key: 'tempoEstimado',
      width: 130,
      align: 'center' as const,
      render: (value: number) => formatDuration(value),
    },
    {
      title: t('motoRevisaoDetalhes.services.cost'),
      dataIndex: 'custo',
      key: 'custo',
      width: 140,
      align: 'right' as const,
      render: (value: number) => <Text strong>{formatCurrency(value)}</Text>,
    },
  ];

  return (
    <Flex vertical gap="large" style={{ width: '100%' }}>
      <Flex vertical gap="middle">
        <DashboardBreadcrumb
          userType="cliente"
          items={[
            { title: t('minhasMotos.title'), path: PATHS.CLIENTE_MOTOS, icon: <CarOutlined /> },
            { title: `${moto.marca} ${moto.nomeModelo}` },
            { title: getRevisionTitle(revisao) },
          ]}
        />

        <Flex align="center" gap="middle" wrap="wrap">
          <Button
            type="default"
            icon={<ArrowLeftOutlined />}
            onClick={onBack}
            aria-label={t('back')}
          />
          <Flex vertical gap={2}>
            <Title level={2} style={{ margin: 0 }}>
              {getRevisionTitle(revisao)} - {moto.marca} {moto.nomeModelo}
            </Title>
            <Flex align="center" gap={8} wrap="wrap">
              <Badge status={statusConfig.badgeStatus} text={statusConfig.label} />
              <Text type="secondary" style={{ fontSize: 12 }}>
                {t('motoRevisaoDetalhes.plan', { line: moto.linha })}
              </Text>
            </Flex>
          </Flex>
        </Flex>
      </Flex>

      {status === 'atrasada' && (
        <Alert
          type="error"
          showIcon
          icon={<WarningFilled />}
          message={t('motoRevisaoDetalhes.alert.overdue.title')}
          description={t('motoRevisaoDetalhes.alert.overdue.description')}
        />
      )}

      <Card title={t('motoRevisaoDetalhes.deadline.title')}>
        <Flex vertical gap="middle">
          <Descriptions size="small" column={{ xs: 1, sm: 2, md: 4 }}>
            <Descriptions.Item label={t('motoRevisaoDetalhes.deadline.idealDate')}>
              <Tag color="blue">{formatDate(revisao.dataPrevista)}</Tag>
            </Descriptions.Item>
            <Descriptions.Item label={t('motoRevisaoDetalhes.deadline.months')}>
              {t('motoRevisaoDetalhes.deadline.monthsValue', { months: revisao.tempoMeses })}
            </Descriptions.Item>
            <Descriptions.Item label={t('motoRevisaoDetalhes.deadline.mileage')}>
              <Tag color="purple">{formatNumber(revisao.quilometragem)} km</Tag>
            </Descriptions.Item>
            <Descriptions.Item label={t('motoRevisaoDetalhes.deadline.status')}>
              <Tag color={statusConfig.tagColor}>{statusConfig.label}</Tag>
            </Descriptions.Item>
          </Descriptions>

          <Alert
            type={status === 'atrasada' ? 'error' : 'info'}
            showIcon
            message={
              daysUntilRevision >= 0
                ? t('motoRevisaoDetalhes.deadline.daysRemaining', { days: daysUntilRevision })
                : t('motoRevisaoDetalhes.deadline.daysOverdue', { days: Math.abs(daysUntilRevision) })
            }
          />
        </Flex>
      </Card>

      <Card title={t('motoRevisaoDetalhes.info.title')}>
        <Descriptions size="small" column={{ xs: 1, sm: 2, md: 4 }}>
          <Descriptions.Item label={t('motoRevisaoDetalhes.info.revision')}>{getRevisionTitle(revisao)}</Descriptions.Item>
          <Descriptions.Item label={t('motoRevisaoDetalhes.info.estimatedTime')}>{formatDuration(totalTempo)}</Descriptions.Item>
          <Descriptions.Item label={t('motoRevisaoDetalhes.info.parts')}>{revisao.pecas?.length ?? 0}</Descriptions.Item>
          <Descriptions.Item label={t('motoRevisaoDetalhes.info.services')}>{revisao.servicos?.length ?? 0}</Descriptions.Item>
          <Descriptions.Item label={t('motoRevisaoDetalhes.info.estimatedCost')}>
            <Text strong style={{ color: token.colorPrimary }}>{formatCurrency(totalGeral)}</Text>
          </Descriptions.Item>
        </Descriptions>
      </Card>

      <Card
        title={
          <Flex align="center" gap={8}>
            <TagOutlined />
            <span>{t('motoRevisaoDetalhes.parts.title')}</span>
            <Tag>{revisao.pecas?.length ?? 0}</Tag>
          </Flex>
        }
      >
        <Table
          dataSource={revisao.pecas ?? []}
          columns={pecasColumns}
          rowKey="id"
          pagination={false}
          size="small"
          locale={{ emptyText: t('motoRevisaoDetalhes.parts.empty') }}
          summary={() => (
            <Table.Summary.Row>
              <Table.Summary.Cell index={0} colSpan={4}>
                <Text strong>{t('motoRevisaoDetalhes.parts.totalParts')}</Text>
              </Table.Summary.Cell>
              <Table.Summary.Cell index={1} align="right">
                <Text strong style={{ color: token.colorPrimary }}>{formatCurrency(totalPecas)}</Text>
              </Table.Summary.Cell>
            </Table.Summary.Row>
          )}
        />
      </Card>

      <Card
        title={
          <Flex align="center" gap={8}>
            <ToolOutlined />
            <span>{t('motoRevisaoDetalhes.services.title')}</span>
            <Tag>{revisao.servicos?.length ?? 0}</Tag>
          </Flex>
        }
      >
        <Table
          dataSource={revisao.servicos ?? []}
          columns={servicosColumns}
          rowKey="id"
          pagination={false}
          size="small"
          locale={{ emptyText: t('motoRevisaoDetalhes.services.empty') }}
          summary={() => (
            <Table.Summary.Row>
              <Table.Summary.Cell index={0}>
                <Text strong>{t('motoRevisaoDetalhes.services.totalServices')}</Text>
              </Table.Summary.Cell>
              <Table.Summary.Cell index={1} align="center">
                <Text strong>{formatDuration(totalTempo)}</Text>
              </Table.Summary.Cell>
              <Table.Summary.Cell index={2} align="right">
                <Text strong style={{ color: token.colorPrimary }}>{formatCurrency(totalServicos)}</Text>
              </Table.Summary.Cell>
            </Table.Summary.Row>
          )}
        />
      </Card>

      <Card size="small">
        <Flex justify="space-between" align="center" wrap="wrap" gap="middle">
          <Flex vertical gap={2}>
            <Text type="secondary" style={{ fontSize: 12 }}>{t('motoRevisaoDetalhes.footer.totalLabel')}</Text>
            <Title level={3} style={{ margin: 0, color: token.colorPrimary }}>
              {formatCurrency(totalGeral)}
            </Title>
            <Text type="secondary" style={{ fontSize: 12 }}>
              {t('motoRevisaoDetalhes.footer.breakdown', {
                parts: formatCurrency(totalPecas),
                services: formatCurrency(totalServicos),
              })}
            </Text>
          </Flex>
        </Flex>
      </Card>
    </Flex>
  );
}

export default function MotoDetalhes() {
  const { token } = theme.useToken();
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [moto, setMoto] = useState<Moto | null>(null);
  const [selectedRevisionId, setSelectedRevisionId] = useState<number | null>(null);
  const [loading, setLoading] = useState(true);
  const [errorStatus, setErrorStatus] = useState<number | null>(null);
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
        setErrorStatus(err instanceof ApiError ? (err.status ?? 500) : 500);
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
            { title: t('motoDetalhes.title') },
          ]}
        />
        <Card>
          <Skeleton active avatar paragraph={{ rows: 8 }} />
        </Card>
      </Flex>
    );
  }

  if (errorStatus) {
    const resultStatus = errorStatus === 403 ? '403' : errorStatus === 404 ? '404' : '500';
    const resultTitle =
      errorStatus === 403
        ? t('motoDetalhes.forbidden.title')
        : errorStatus === 404
          ? t('motoDetalhes.notFound.title')
          : t('motoDetalhes.error.title');
    const resultMessage =
      errorStatus === 403
        ? t('motoDetalhes.forbidden.message')
        : errorStatus === 404
          ? t('motoDetalhes.notFound.message')
          : t('motoDetalhes.error.message');

    return (
      <Flex vertical justify="center" align="center" style={{ width: '100%', minHeight: '60vh' }}>
        <Result
          status={resultStatus}
          title={resultTitle}
          subTitle={resultMessage}
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

  const revisoes = [...(moto.revisoesPlanejadas ?? [])].sort((a, b) => a.ordem - b.ordem);
  const selectedRevision = selectedRevisionId
    ? revisoes.find((revisao) => revisao.id === selectedRevisionId)
    : null;

  if (selectedRevision) {
    return (
      <RevisaoDetalhes
        moto={moto}
        revisao={selectedRevision}
        onBack={() => setSelectedRevisionId(null)}
      />
    );
  }

  const revisionsWithStatus = revisoes.map((revisao) => ({ revisao, status: getRevisionStatus(revisao) }));
  const counts = {
    concluida: revisionsWithStatus.filter((item) => item.status === 'concluida').length,
    agendada: revisionsWithStatus.filter((item) => item.status === 'agendada').length,
    emExecucao: revisionsWithStatus.filter((item) => item.status === 'em_execucao').length,
    atrasada: revisionsWithStatus.filter((item) => item.status === 'atrasada').length,
    planejada: revisionsWithStatus.filter((item) => item.status === 'planejada').length,
  };
  const proximaRevisao = revisionsWithStatus.find((item) => item.status !== 'concluida')?.revisao;
  const totalEstimado = revisoes.reduce((total, revisao) => total + getRevisionEstimate(revisao), 0);
  const dataVendaFormatada = formatDate(moto.dataVenda);

  const timelineItems = revisionsWithStatus.map(({ revisao, status }) => ({
    dot: getStatusConfig(status, token).icon,
    children: (
      <RevisaoPlanejadaCard
        revisao={revisao}
        onClick={() => setSelectedRevisionId(revisao.id)}
      />
    ),
  }));

  return (
    <Flex vertical gap="large" style={{ width: '100%' }}>
      <Flex vertical gap="middle">
        <DashboardBreadcrumb
          userType="cliente"
          items={[
            { title: t('minhasMotos.title'), path: PATHS.CLIENTE_MOTOS, icon: <CarOutlined /> },
            { title: `${moto.marca} ${moto.nomeModelo}` },
          ]}
        />

        <Flex justify="space-between" align="center" wrap="wrap" gap="middle">
          <Flex align="center" gap="middle">
            <Button
              type="default"
              icon={<ArrowLeftOutlined />}
              onClick={() => navigate(PATHS.CLIENTE_MOTOS)}
              aria-label={t('back')}
            />
            <Title level={2} style={{ margin: 0 }}>
              {moto.marca} {moto.nomeModelo}
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
            <Button danger type="primary" icon={<DeleteOutlined />} loading={deleting}>
              {t('motoDetalhes.remove')}
            </Button>
          </Popconfirm>
        </Flex>
      </Flex>

      {counts.atrasada > 0 && (
        <Alert
          type="error"
          showIcon
          icon={<WarningFilled />}
          message={t('motoDetalhes.alert.overdue.title', { count: counts.atrasada })}
          description={t('motoDetalhes.alert.overdue.description')}
        />
      )}

      <Card>
        <Flex gap="large" wrap="wrap">
          <Flex
            justify="center"
            align="center"
            style={{
              width: '100%',
              maxWidth: 380,
              height: 230,
              borderRadius: token.borderRadiusLG,
              overflow: 'hidden',
              background: `linear-gradient(135deg, ${token.colorBgLayout} 0%, ${token.colorFillSecondary} 100%)`,
              flexShrink: 0,
            }}
          >
            {moto.foto ? (
              <img
                src={getImageUrl(moto.foto)}
                alt={`${moto.marca} ${moto.nomeModelo}`}
                style={{ width: '100%', height: '100%', objectFit: 'cover' }}
              />
            ) : (
              <CarOutlined style={{ fontSize: 72, color: token.colorTextQuaternary }} />
            )}
          </Flex>

          <Flex vertical gap="middle" style={{ flex: 1, minWidth: 260 }}>
            <Flex vertical gap={6}>
              <Title level={3} style={{ margin: 0 }}>
                {moto.marca} {moto.nomeModelo}
              </Title>
              <Flex gap={6} wrap="wrap">
                <Tag>{moto.ano}</Tag>
                <Tag color="blue">{moto.linha}</Tag>
                <Tag color="purple">{moto.cilindrada}</Tag>
                <Tag>{moto.cor}</Tag>
              </Flex>
            </Flex>

            <Divider style={{ margin: '4px 0' }} />

            <Flex gap="large" wrap="wrap">
              <Flex vertical gap={2}>
                <Text type="secondary" style={{ fontSize: 11 }}>{t('motoDetalhes.label.placa').toUpperCase()}</Text>
                <Text strong style={{ fontSize: 16, letterSpacing: 1 }}>{moto.placa}</Text>
              </Flex>
              <Flex vertical gap={2}>
                <Text type="secondary" style={{ fontSize: 11 }}>{t('motoDetalhes.label.kilometragem').toUpperCase()}</Text>
                <Flex align="center" gap={4}>
                  <DashboardOutlined style={{ color: token.colorTextTertiary }} />
                  <Text strong style={{ fontSize: 16 }}>{formatNumber(moto.kilometragemAtual)} km</Text>
                </Flex>
              </Flex>
              <Flex vertical gap={2}>
                <Text type="secondary" style={{ fontSize: 11 }}>{t('motoDetalhes.label.dataVenda').toUpperCase()}</Text>
                <Flex align="center" gap={4}>
                  <CalendarOutlined style={{ color: token.colorTextTertiary }} />
                  <Text strong>{dataVendaFormatada}</Text>
                </Flex>
              </Flex>
            </Flex>

            <Flex align="center" gap={6}>
              <TagOutlined style={{ color: token.colorTextTertiary, fontSize: 12 }} />
              <Text type="secondary" style={{ fontSize: 12 }}>{t('minhasMotos.card.chassi', { chassi: moto.chassi })}</Text>
            </Flex>
          </Flex>
        </Flex>
      </Card>

      <Flex gap="middle" wrap="wrap">
        <Card size="small" style={{ flex: '1 1 120px', textAlign: 'center' }}>
          <Statistic title={t('motoDetalhes.stats.done')} value={counts.concluida} prefix={<CheckCircleFilled />} valueStyle={{ color: token.colorSuccess, fontSize: 22 }} />
        </Card>
        <Card size="small" style={{ flex: '1 1 120px', textAlign: 'center' }}>
          <Statistic title={t('motoDetalhes.stats.planned')} value={counts.planejada} prefix={<HourglassOutlined />} valueStyle={{ color: token.colorTextSecondary, fontSize: 22 }} />
        </Card>
        <Card size="small" style={{ flex: '1 1 120px', textAlign: 'center' }}>
          <Statistic title={t('motoDetalhes.stats.overdue')} value={counts.atrasada} prefix={<CloseCircleFilled />} valueStyle={{ color: counts.atrasada > 0 ? token.colorError : token.colorTextSecondary, fontSize: 22 }} />
        </Card>
        <Card size="small" style={{ flex: '2 1 240px' }}>
          <Flex vertical gap={2}>
            <Text type="secondary" style={{ fontSize: 11 }}>{t('motoDetalhes.stats.next').toUpperCase()}</Text>
            {proximaRevisao ? (
              <>
                <Text strong>{proximaRevisao.nome}</Text>
                <Text type="secondary" style={{ fontSize: 12 }}>
                  {formatNumber(proximaRevisao.quilometragem)} km · {formatDate(proximaRevisao.dataPrevista)}
                </Text>
              </>
            ) : (
              <Text type="secondary">{t('motoDetalhes.stats.noNext')}</Text>
            )}
          </Flex>
        </Card>
      </Flex>

      <Card
        title={
          <Flex align="center" gap={8} wrap="wrap">
            <ClockCircleFilled style={{ color: token.colorPrimary }} />
            <span>{t('motoDetalhes.revisions.title')}</span>
            <Tag>{moto.linha}</Tag>
          </Flex>
        }
        extra={revisoes.length > 0 ? t('motoDetalhes.revisions.totalEstimate', { value: formatCurrency(totalEstimado) }) : undefined}
      >
        {timelineItems.length === 0 ? (
          <Empty description={t('motoDetalhes.revisions.empty')} />
        ) : (
          <Timeline items={timelineItems} style={{ marginTop: 16 }} />
        )}
      </Card>
    </Flex>
  );
}
