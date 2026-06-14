import { useEffect, useMemo, useState, type CSSProperties, type ReactNode } from 'react';
import {
  Alert,
  Button,
  Card,
  Col,
  Empty,
  Flex,
  Result,
  Row,
  Spin,
  Tag,
  Tooltip,
  Typography,
  theme,
} from 'antd';
import {
  CalendarOutlined,
  CarOutlined,
  ClockCircleOutlined,
  ExclamationCircleOutlined,
  HourglassOutlined,
  ShopOutlined,
  SyncOutlined,
  ToolOutlined,
} from '@ant-design/icons';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { getLocale, t } from '@/app/i18n';
import { AgendamentoCliente, StatusAgendamentoCliente } from '@/app/models/AgendamentoCliente';
import { PATHS } from '@/app/paths';
import { agendamentoService } from '@/app/services/agendamentoService';
import { handleApiError } from '@/app/utils/errorHandler';

const { Title, Text } = Typography;

type ThemeToken = ReturnType<typeof theme.useToken>['token'];

const STATUS = {
  AGUARDANDO_AGENDAMENTO: 'aguardando_agendamento',
  AGUARDANDO_CONFIRMACAO: 'aguardando_confirmacao',
  AGENDADA: 'agendada',
  EM_EXECUCAO: 'em_execucao',
  ATRASADA: 'atrasada',
} as const satisfies Record<string, StatusAgendamentoCliente>;

const formatDate = (date?: string | null) => {
  if (!date) return '-';
  return new Date(`${date.substring(0, 10)}T00:00:00`).toLocaleDateString(getLocale());
};

const startOfToday = () => {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return today;
};

const daysUntil = (date: string) => {
  const target = new Date(`${date.substring(0, 10)}T00:00:00`);
  return Math.ceil((target.getTime() - startOfToday().getTime()) / (1000 * 60 * 60 * 24));
};

const getDeadlineText = (item: AgendamentoCliente) => {
  if (item.status === STATUS.EM_EXECUCAO) return t('clienteAgendamentos.deadline.inProgress');
  if (item.status === STATUS.AGUARDANDO_CONFIRMACAO) return t('clienteAgendamentos.deadline.awaitingConfirmation');

  if (item.status === STATUS.AGENDADA) {
    const days = daysUntil(item.dataIdeal);
    if (days > 0) return t('clienteAgendamentos.deadline.untilIdeal', { days });
    if (days === 0) return t('clienteAgendamentos.deadline.idealToday');
    return t('clienteAgendamentos.deadline.afterIdeal', { days: Math.abs(days) });
  }

  if (item.status === STATUS.ATRASADA) {
    return t('clienteAgendamentos.deadline.late', { days: Math.abs(daysUntil(item.dataIdeal)) });
  }

  const days = daysUntil(item.dataLimite);
  if (days === 0) return t('clienteAgendamentos.deadline.lastDay');
  return t('clienteAgendamentos.deadline.untilLimit', { days });
};

const getStatusConfig = (status: StatusAgendamentoCliente, token: ThemeToken) => {
  const configs = {
    aguardando_agendamento: {
      label: t('clienteAgendamentos.status.awaitingSchedule'),
      tagColor: 'default',
      accent: token.colorWarning,
      icon: <HourglassOutlined />,
    },
    aguardando_confirmacao: {
      label: t('clienteAgendamentos.status.awaitingConfirmation'),
      tagColor: 'processing',
      accent: token.colorInfo,
      icon: <ClockCircleOutlined />,
    },
    agendada: {
      label: t('clienteAgendamentos.status.scheduled'),
      tagColor: 'blue',
      accent: token.colorPrimary,
      icon: <CalendarOutlined />,
    },
    em_execucao: {
      label: t('clienteAgendamentos.status.inProgress'),
      tagColor: 'cyan',
      accent: token.colorInfo,
      icon: <ToolOutlined />,
    },
    atrasada: {
      label: t('clienteAgendamentos.status.late'),
      tagColor: 'error',
      accent: token.colorError,
      icon: <ExclamationCircleOutlined />,
    },
  };

  return configs[status];
};

function SummaryCard({
  title,
  value,
  icon,
  color,
}: {
  title: string;
  value: number;
  icon: ReactNode;
  color: string;
}) {
  const { token } = theme.useToken();

  return (
    <Card styles={{ body: { padding: 16 } }}>
      <Flex justify="space-between" align="center" gap="middle">
        <Flex vertical gap={4}>
          <Text type="secondary" style={{ fontSize: 12 }}>{title}</Text>
          <Title level={3} style={{ margin: 0, color }}>{value}</Title>
        </Flex>
        <Flex
          align="center"
          justify="center"
          style={{
            width: 40,
            height: 40,
            borderRadius: token.borderRadiusLG,
            background: token.colorFillSecondary,
            color,
          }}
        >
          {icon}
        </Flex>
      </Flex>
    </Card>
  );
}

function CardInfo({
  label,
  value,
}: {
  label: string;
  value: ReactNode;
}) {
  return (
    <Flex vertical gap={2}>
      <Text type="secondary" style={{ fontSize: 12 }}>{label}</Text>
      <Text>{value}</Text>
    </Flex>
  );
}

function ActionButtons({ item }: { item: AgendamentoCliente }) {
  const disabledReason = t('clienteAgendamentos.actions.soon');

  if (item.status === STATUS.EM_EXECUCAO) {
    return (
      <Tooltip title={disabledReason}>
        <Button type="primary" disabled icon={<ToolOutlined />}>
          {t('clienteAgendamentos.actions.follow')}
        </Button>
      </Tooltip>
    );
  }

  if (item.status === STATUS.AGENDADA) {
    return (
      <Flex gap="small" wrap="wrap">
        <Tooltip title={disabledReason}>
          <Button disabled>{t('clienteAgendamentos.actions.cancel')}</Button>
        </Tooltip>
        <Tooltip title={disabledReason}>
          <Button type="primary" disabled icon={<SyncOutlined />}>
            {t('clienteAgendamentos.actions.reschedule')}
          </Button>
        </Tooltip>
      </Flex>
    );
  }

  if (item.status === STATUS.ATRASADA) {
    return (
      <Tooltip title={disabledReason}>
        <Button type="primary" danger disabled icon={<SyncOutlined />}>
          {t('clienteAgendamentos.actions.rescheduleLate')}
        </Button>
      </Tooltip>
    );
  }

  if (item.status === STATUS.AGUARDANDO_AGENDAMENTO) {
    return (
      <Tooltip title={disabledReason}>
        <Button type="primary" disabled icon={<CalendarOutlined />}>
          {t('clienteAgendamentos.actions.schedule')}
        </Button>
      </Tooltip>
    );
  }

  return null;
}

function StatusAlert({ item }: { item: AgendamentoCliente }) {
  if (item.status === STATUS.ATRASADA) {
    return (
      <Alert
        type="error"
        showIcon
        message={t('clienteAgendamentos.alerts.late.title')}
        description={t('clienteAgendamentos.alerts.late.description', { deadline: formatDate(item.dataLimite) })}
      />
    );
  }

  if (item.status === STATUS.AGUARDANDO_CONFIRMACAO) {
    return (
      <Alert
        type="info"
        showIcon
        message={t('clienteAgendamentos.alerts.awaitingConfirmation.title')}
        description={t('clienteAgendamentos.alerts.awaitingConfirmation.description', {
          date: formatDate(item.dataAgendada),
        })}
      />
    );
  }

  if (item.status === STATUS.EM_EXECUCAO) {
    return (
      <Alert
        type="info"
        showIcon
        message={t('clienteAgendamentos.alerts.inProgress.title')}
        description={t('clienteAgendamentos.alerts.inProgress.description')}
      />
    );
  }

  return null;
}

function AgendamentoCard({ item }: { item: AgendamentoCliente }) {
  const { token } = theme.useToken();
  const statusConfig = getStatusConfig(item.status, token);
  const cardStyle: CSSProperties = {
    height: '100%',
    borderTop: `3px solid ${statusConfig.accent}`,
  };
  const dateLabel =
    item.status === STATUS.AGUARDANDO_CONFIRMACAO
      ? t('clienteAgendamentos.card.requestedDate')
      : item.status === STATUS.EM_EXECUCAO
        ? t('clienteAgendamentos.card.shopEntry')
        : t('clienteAgendamentos.card.scheduledDate');
  const shouldShowScheduleDate =
    item.dataAgendada &&
    [STATUS.AGUARDANDO_CONFIRMACAO, STATUS.AGENDADA, STATUS.EM_EXECUCAO, STATUS.ATRASADA].includes(item.status);

  return (
    <Card hoverable style={cardStyle} styles={{ body: { height: '100%' } }}>
      <Flex vertical gap="middle" style={{ height: '100%' }}>
        <Flex justify="space-between" align="flex-start" gap="small">
          <Flex vertical gap={4} style={{ minWidth: 0 }}>
            <Text strong style={{ fontSize: 16 }} ellipsis={{ tooltip: `${item.marca} ${item.modelo}` }}>
              {item.marca} {item.modelo}
            </Text>
            <Text type="secondary" style={{ fontSize: 12 }}>{item.placa}</Text>
          </Flex>
          <Tag icon={statusConfig.icon} color={statusConfig.tagColor} style={{ marginInlineEnd: 0 }}>
            {statusConfig.label}
          </Tag>
        </Flex>

        <Flex vertical gap={4}>
          <Text strong>
            {t('clienteAgendamentos.card.revisionTitle', { number: item.numeroRevisao })}
          </Text>
          <Text type="secondary">{item.nomeRevisao}</Text>
        </Flex>

        <Row gutter={[16, 12]}>
          <Col xs={24} sm={12}>
            <CardInfo label={t('clienteAgendamentos.card.idealDate')} value={formatDate(item.dataIdeal)} />
          </Col>
          {shouldShowScheduleDate && (
            <Col xs={24} sm={12}>
              <CardInfo label={dateLabel} value={formatDate(item.dataAgendada)} />
            </Col>
          )}
          {[STATUS.AGUARDANDO_AGENDAMENTO, STATUS.ATRASADA].includes(item.status) && (
            <Col xs={24} sm={12}>
              <CardInfo label={t('clienteAgendamentos.card.deadline')} value={formatDate(item.dataLimite)} />
            </Col>
          )}
          {item.nomeLoja && (
            <Col xs={24}>
              <CardInfo
                label={t('clienteAgendamentos.card.dealership')}
                value={
                  <Flex align="center" gap={6} wrap="wrap">
                    <ShopOutlined style={{ color: token.colorTextTertiary }} />
                    <span>
                      {item.nomeLoja}
                      {item.cidadeLoja ? ` - ${item.cidadeLoja}` : ''}
                    </span>
                  </Flex>
                }
              />
            </Col>
          )}
          <Col xs={24}>
            <CardInfo
              label={t('clienteAgendamentos.card.items')}
              value={t('clienteAgendamentos.card.itemsValue', {
                parts: item.quantidadePecas,
                services: item.quantidadeServicos,
              })}
            />
          </Col>
        </Row>

        <StatusAlert item={item} />

        <Flex justify="space-between" align="center" gap="middle" wrap="wrap" style={{ marginTop: 'auto' }}>
          <Text type={item.status === STATUS.ATRASADA ? 'danger' : 'secondary'}>
            {getDeadlineText(item)}
          </Text>
          <ActionButtons item={item} />
        </Flex>
      </Flex>
    </Card>
  );
}

function AgendamentoSection({
  title,
  items,
}: {
  title: string;
  items: AgendamentoCliente[];
}) {
  if (items.length === 0) return null;

  return (
    <Flex vertical gap="middle">
      <Flex align="center" gap="small">
        <Title level={4} style={{ margin: 0 }}>{title}</Title>
        <Tag>{items.length}</Tag>
      </Flex>
      <Row gutter={[16, 16]}>
        {items.map((item) => (
          <Col key={item.revisaoMotoId} xs={24} lg={12} xl={8}>
            <AgendamentoCard item={item} />
          </Col>
        ))}
      </Row>
    </Flex>
  );
}

export default function AgendamentosCliente() {
  const { token } = theme.useToken();
  const [agendamentos, setAgendamentos] = useState<AgendamentoCliente[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState(false);

  useEffect(() => {
    const carregarAgendamentos = async () => {
      try {
        setLoading(true);
        setLoadError(false);
        const data = await agendamentoService.getCliente();
        setAgendamentos(data);
      } catch (error) {
        setLoadError(true);
        handleApiError(error, 'clienteAgendamentos.load.error');
      } finally {
        setLoading(false);
      }
    };

    carregarAgendamentos();
  }, []);

  const grupos = useMemo(() => ({
    emExecucao: agendamentos.filter((item) => item.status === STATUS.EM_EXECUCAO),
    aguardandoConfirmacao: agendamentos.filter((item) => item.status === STATUS.AGUARDANDO_CONFIRMACAO),
    agendadas: agendamentos.filter((item) => item.status === STATUS.AGENDADA),
    disponiveis: agendamentos.filter((item) =>
      [STATUS.AGUARDANDO_AGENDAMENTO, STATUS.ATRASADA].includes(item.status)
    ),
  }), [agendamentos]);

  if (loadError && !loading) {
    return (
      <Flex vertical justify="center" align="center" style={{ width: '100%', minHeight: '60vh' }}>
        <Result
          status="500"
          title={t('clienteAgendamentos.error.title')}
          subTitle={t('clienteAgendamentos.error.message')}
          extra={
            <Button type="primary" onClick={() => window.location.reload()}>
              {t('clienteAgendamentos.error.reload')}
            </Button>
          }
        />
      </Flex>
    );
  }

  return (
    <Spin spinning={loading}>
      <Flex vertical gap="large" style={{ width: '100%' }}>
        <Flex vertical gap="middle">
          <DashboardBreadcrumb
            userType="cliente"
            items={[
              { title: t('clienteAgendamentos.title'), path: PATHS.CLIENTE_AGENDAMENTOS, icon: <CalendarOutlined /> },
            ]}
          />

          <Flex vertical gap={4}>
            <Title level={2} style={{ margin: 0 }}>{t('clienteAgendamentos.title')}</Title>
            <Text type="secondary">{t('clienteAgendamentos.subtitle')}</Text>
          </Flex>
        </Flex>

        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} lg={6}>
            <SummaryCard
              title={t('clienteAgendamentos.summary.inProgress')}
              value={grupos.emExecucao.length}
              color={token.colorInfo}
              icon={<ToolOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <SummaryCard
              title={t('clienteAgendamentos.summary.scheduled')}
              value={grupos.agendadas.length}
              color={token.colorPrimary}
              icon={<CalendarOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <SummaryCard
              title={t('clienteAgendamentos.summary.awaitingConfirmation')}
              value={grupos.aguardandoConfirmacao.length}
              color={token.colorInfo}
              icon={<ClockCircleOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <SummaryCard
              title={t('clienteAgendamentos.summary.available')}
              value={grupos.disponiveis.length}
              color={token.colorWarning}
              icon={<HourglassOutlined />}
            />
          </Col>
        </Row>

        {agendamentos.length === 0 ? (
          <Card>
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description={t('clienteAgendamentos.empty')}
            >
              <Button type="primary" href={PATHS.CLIENTE_MOTOS}>
                <CarOutlined />
                {t('clienteAgendamentos.empty.goToMotorcycles')}
              </Button>
            </Empty>
          </Card>
        ) : (
          <Flex vertical gap="large">
            <AgendamentoSection
              title={t('clienteAgendamentos.sections.inProgress')}
              items={grupos.emExecucao}
            />
            <AgendamentoSection
              title={t('clienteAgendamentos.sections.awaitingConfirmation')}
              items={grupos.aguardandoConfirmacao}
            />
            <AgendamentoSection
              title={t('clienteAgendamentos.sections.scheduled')}
              items={grupos.agendadas}
            />
            <AgendamentoSection
              title={t('clienteAgendamentos.sections.available')}
              items={grupos.disponiveis}
            />
          </Flex>
        )}
      </Flex>
    </Spin>
  );
}
