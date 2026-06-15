import { useCallback, useEffect, useMemo, useState, type CSSProperties, type ReactNode } from 'react';
import { useNavigate } from 'react-router';
import {
  Alert,
  Button,
  Card,
  Col,
  DatePicker,
  Empty,
  Flex,
  Form,
  message,
  Modal,
  Popconfirm,
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
import dayjs, { type Dayjs } from 'dayjs';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { getLocale, t } from '@/app/i18n';
import { AgendamentoCliente, StatusAgendamentoCliente } from '@/app/models/AgendamentoCliente';
import { Loja } from '@/app/models/Loja';
import { PATHS } from '@/app/paths';
import { agendamentoService } from '@/app/services/agendamentoService';
import { concessionariaService } from '@/app/services/concessionariaService';
import { handleApiError } from '@/app/utils/errorHandler';
import { buildMotoRevisionDetailsPath } from '@/app/pages/cliente/revisoes/RevisoesCliente.utils';
import AgendamentoSolicitacaoModal from '@/app/components/cliente/AgendamentoSolicitacaoModal';

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

function RemarcarAgendamentoModal({
  item,
  open,
  confirming,
  onConfirm,
  onCancel,
}: {
  item: AgendamentoCliente | null;
  open: boolean;
  confirming: boolean;
  onConfirm: (novaData: string) => Promise<void>;
  onCancel: () => void;
}) {
  const [form] = Form.useForm<{ novaData: Dayjs }>();

  useEffect(() => {
    if (!open) {
      form.resetFields();
      return;
    }

    if (item?.dataAgendada) {
      form.setFieldsValue({ novaData: dayjs(item.dataAgendada.substring(0, 10)) });
    }
  }, [form, item, open]);

  const disabledDate = (date: Dayjs) => {
    if (!item) return true;

    const data = date.startOf('day');
    const hoje = dayjs().startOf('day');
    const minima = dayjs(item.dataMinima.substring(0, 10)).startOf('day');
    const limite = dayjs(item.dataLimite.substring(0, 10)).endOf('day');

    return data.isBefore(hoje) || data.isBefore(minima) || data.isAfter(limite);
  };

  const handleOk = async () => {
    const values = await form.validateFields();
    await onConfirm(values.novaData.format('YYYY-MM-DD'));
  };

  const handleCancel = () => {
    form.resetFields();
    onCancel();
  };

  return (
    <Modal
      title={t('clienteAgendamentos.rescheduleModal.title')}
      open={open}
      onOk={handleOk}
      onCancel={handleCancel}
      okText={t('clienteAgendamentos.rescheduleModal.ok')}
      cancelText={t('clienteAgendamentos.rescheduleModal.cancel')}
      confirmLoading={confirming}
      destroyOnHidden
    >
      {item && (
        <Flex vertical gap="middle">
          <Alert
            type="info"
            showIcon
            message={t('clienteAgendamentos.rescheduleModal.alert.title', {
              motorcycle: `${item.marca} ${item.modelo}`,
              revision: item.numeroRevisao,
            })}
            description={t('clienteAgendamentos.rescheduleModal.alert.description', {
              start: formatDate(item.dataMinima),
              end: formatDate(item.dataLimite),
            })}
          />

          {item.nomeLoja && (
            <CardInfo
              label={t('clienteAgendamentos.card.dealership')}
              value={`${item.nomeLoja}${item.cidadeLoja ? ` - ${item.cidadeLoja}` : ''}`}
            />
          )}

          <Form form={form} layout="vertical">
            <Form.Item
              label={t('clienteAgendamentos.rescheduleModal.date.label')}
              name="novaData"
              rules={[{ required: true, message: t('clienteAgendamentos.rescheduleModal.date.required') }]}
            >
              <DatePicker
                style={{ width: '100%' }}
                format="DD/MM/YYYY"
                disabledDate={disabledDate}
                placeholder={t('clienteAgendamentos.rescheduleModal.date.placeholder')}
              />
            </Form.Item>
          </Form>

          <Text type="secondary">
            {t('clienteAgendamentos.rescheduleModal.footer')}
          </Text>
        </Flex>
      )}
    </Modal>
  );
}

function ActionButtons({
  item,
  actionLoadingId,
  onCancel,
  onOpenReschedule,
  onOpenSchedule,
  onFollow,
}: {
  item: AgendamentoCliente;
  actionLoadingId: number | null;
  onCancel: (item: AgendamentoCliente) => Promise<void>;
  onOpenReschedule: (item: AgendamentoCliente) => void;
  onOpenSchedule: (item: AgendamentoCliente) => void;
  onFollow: (item: AgendamentoCliente) => void;
}) {
  const hasAgendamento = Boolean(item.agendamentoId);
  const loading = Boolean(item.agendamentoId && actionLoadingId === item.agendamentoId);

  if (item.status === STATUS.EM_EXECUCAO) {
    return (
      <Button type="primary" icon={<ToolOutlined />} onClick={() => onFollow(item)}>
        {t('clienteAgendamentos.actions.follow')}
      </Button>
    );
  }

  if (item.status === STATUS.AGENDADA) {
    return (
      <Flex gap="small" wrap="wrap">
        <Tooltip title={hasAgendamento ? undefined : t('clienteAgendamentos.actions.unavailable')}>
          <Popconfirm
            title={t('clienteAgendamentos.cancelConfirm.title')}
            description={t('clienteAgendamentos.cancelConfirm.description')}
            icon={<ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />}
            okText={t('clienteAgendamentos.cancelConfirm.ok')}
            cancelText={t('clienteAgendamentos.cancelConfirm.cancel')}
            okButtonProps={{ danger: true, loading }}
            onConfirm={() => onCancel(item)}
            disabled={!hasAgendamento || loading}
          >
            <Button danger disabled={!hasAgendamento} loading={loading}>
              {t('clienteAgendamentos.actions.cancel')}
            </Button>
          </Popconfirm>
        </Tooltip>
        <Tooltip title={hasAgendamento ? undefined : t('clienteAgendamentos.actions.unavailable')}>
          <Button
            type="primary"
            disabled={!hasAgendamento || loading}
            loading={loading}
            icon={<SyncOutlined />}
            onClick={() => onOpenReschedule(item)}
          >
            {t('clienteAgendamentos.actions.reschedule')}
          </Button>
        </Tooltip>
      </Flex>
    );
  }

  if (item.status === STATUS.ATRASADA) {
    return (
      <Button type="primary" danger icon={<SyncOutlined />} onClick={() => onOpenSchedule(item)}>
        {t('clienteAgendamentos.actions.rescheduleLate')}
      </Button>
    );
  }

  if (item.status === STATUS.AGUARDANDO_AGENDAMENTO) {
    return (
      <Button type="primary" icon={<CalendarOutlined />} onClick={() => onOpenSchedule(item)}>
        {t('clienteAgendamentos.actions.schedule')}
      </Button>
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

function AgendamentoCard({
  item,
  actionLoadingId,
  onCancel,
  onOpenReschedule,
  onOpenSchedule,
  onFollow,
}: {
  item: AgendamentoCliente;
  actionLoadingId: number | null;
  onCancel: (item: AgendamentoCliente) => Promise<void>;
  onOpenReschedule: (item: AgendamentoCliente) => void;
  onOpenSchedule: (item: AgendamentoCliente) => void;
  onFollow: (item: AgendamentoCliente) => void;
}) {
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
          <ActionButtons
            item={item}
            actionLoadingId={actionLoadingId}
            onCancel={onCancel}
            onOpenReschedule={onOpenReschedule}
            onOpenSchedule={onOpenSchedule}
            onFollow={onFollow}
          />
        </Flex>
      </Flex>
    </Card>
  );
}

function AgendamentoSection({
  title,
  icon,
  items,
  actionLoadingId,
  onCancel,
  onOpenReschedule,
  onOpenSchedule,
  onFollow,
}: {
  title: string;
  icon: ReactNode;
  items: AgendamentoCliente[];
  actionLoadingId: number | null;
  onCancel: (item: AgendamentoCliente) => Promise<void>;
  onOpenReschedule: (item: AgendamentoCliente) => void;
  onOpenSchedule: (item: AgendamentoCliente) => void;
  onFollow: (item: AgendamentoCliente) => void;
}) {
  return (
    <Flex
      vertical
      gap="middle"
      style={{
        minWidth: 300,
        flex: '1 1 0',
      }}
    >
      <Flex
        justify="space-between"
        align="center"
        gap="small"
      >
        <Flex align="center" gap={8} style={{ minWidth: 0 }}>
          {icon}
          <Title level={4} style={{ margin: 0 }} ellipsis={{ tooltip: title }}>
            {title}
          </Title>
        </Flex>
        <Tag style={{ marginInlineEnd: 0 }}>{items.length}</Tag>
      </Flex>

      <Flex vertical gap="middle">
        {items.map((item) => (
          <AgendamentoCard
            key={item.revisaoMotoId}
            item={item}
            actionLoadingId={actionLoadingId}
            onCancel={onCancel}
            onOpenReschedule={onOpenReschedule}
            onOpenSchedule={onOpenSchedule}
            onFollow={onFollow}
          />
        ))}

        {items.length === 0 && (
          <Card styles={{ body: { padding: 16, textAlign: 'center' } }}>
            <Text type="secondary">{t('clienteAgendamentos.empty.column')}</Text>
          </Card>
        )}
      </Flex>
    </Flex>
  );
}

export default function AgendamentosCliente() {
  const { token } = theme.useToken();
  const navigate = useNavigate();
  const [agendamentos, setAgendamentos] = useState<AgendamentoCliente[]>([]);
  const [lojas, setLojas] = useState<Loja[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState(false);
  const [actionLoadingId, setActionLoadingId] = useState<number | null>(null);
  const [remarcarItem, setRemarcarItem] = useState<AgendamentoCliente | null>(null);
  const [agendarItem, setAgendarItem] = useState<AgendamentoCliente | null>(null);
  const [remarcando, setRemarcando] = useState(false);
  const [agendando, setAgendando] = useState(false);

  const carregarAgendamentos = useCallback(async (showLoading = true) => {
    try {
      if (showLoading) {
        setLoading(true);
      }
      setLoadError(false);
      const data = await agendamentoService.getCliente();
      setAgendamentos(data);
    } catch (error) {
      setLoadError(true);
      handleApiError(error, 'clienteAgendamentos.load.error');
    } finally {
      if (showLoading) {
        setLoading(false);
      }
    }
  }, []);

  useEffect(() => {
    carregarAgendamentos();
  }, [carregarAgendamentos]);

  useEffect(() => {
    const carregarLojas = async () => {
      try {
        const data = await concessionariaService.getLojasAtivas();
        setLojas(data);
      } catch (error) {
        handleApiError(error, 'clienteConcessionarias.load.error');
      }
    };

    carregarLojas();
  }, []);

  const handleCancelar = async (item: AgendamentoCliente) => {
    if (!item.agendamentoId) return;

    try {
      setActionLoadingId(item.agendamentoId);
      await agendamentoService.cancelarCliente(item.agendamentoId);
      message.success(t('clienteAgendamentos.actions.cancel.success'));
      await carregarAgendamentos(false);
    } catch (error) {
      handleApiError(error, 'clienteAgendamentos.actions.cancel.error');
    } finally {
      setActionLoadingId(null);
    }
  };

  const handleRemarcar = async (novaData: string) => {
    if (!remarcarItem?.agendamentoId) return;

    try {
      setRemarcando(true);
      setActionLoadingId(remarcarItem.agendamentoId);
      await agendamentoService.remarcarCliente(remarcarItem.agendamentoId, novaData);
      message.success(t('clienteAgendamentos.actions.reschedule.success'));
      setRemarcarItem(null);
      await carregarAgendamentos(false);
    } catch (error) {
      handleApiError(error, 'clienteAgendamentos.actions.reschedule.error');
    } finally {
      setRemarcando(false);
      setActionLoadingId(null);
    }
  };

  const handleAgendar = async ({ lojaId, dataAgendada }: { lojaId: number; dataAgendada: string }) => {
    if (!agendarItem) return;

    try {
      setAgendando(true);
      await agendamentoService.agendarCliente(agendarItem.revisaoMotoId, lojaId, dataAgendada);
      message.success(t('clienteAgendamentos.actions.schedule.success'));
      setAgendarItem(null);
      await carregarAgendamentos(false);
    } catch (error) {
      handleApiError(error, 'clienteAgendamentos.actions.schedule.error');
    } finally {
      setAgendando(false);
    }
  };

  const handleAcompanhar = (item: AgendamentoCliente) => {
    navigate(buildMotoRevisionDetailsPath(
      PATHS.CLIENTE_MOTOS_DETALHES,
      item.motoId,
      item.revisaoMotoId,
      item.status,
      PATHS.CLIENTE_AGENDAMENTOS
    ));
  };

  const grupos = useMemo(() => ({
    emExecucao: agendamentos.filter((item) => item.status === STATUS.EM_EXECUCAO),
    aguardandoConfirmacao: agendamentos.filter((item) => item.status === STATUS.AGUARDANDO_CONFIRMACAO),
    agendadas: agendamentos.filter((item) => item.status === STATUS.AGENDADA),
    atrasadas: agendamentos.filter((item) => item.status === STATUS.ATRASADA),
    aguardandoAgendamento: agendamentos.filter((item) => item.status === STATUS.AGUARDANDO_AGENDAMENTO),
  }), [agendamentos]);

  const disponiveisCount = grupos.aguardandoAgendamento.length + grupos.atrasadas.length;

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
              value={disponiveisCount}
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
          <Flex
            gap="middle"
            align="stretch"
            style={{
              width: '100%',
              overflowX: 'auto',
              paddingBottom: 8,
            }}
          >
            <AgendamentoSection
              title={t('clienteAgendamentos.status.inProgress')}
              icon={<ToolOutlined style={{ color: token.colorInfo, fontSize: 18 }} />}
              items={grupos.emExecucao}
              actionLoadingId={actionLoadingId}
              onCancel={handleCancelar}
              onOpenReschedule={setRemarcarItem}
              onOpenSchedule={setAgendarItem}
              onFollow={handleAcompanhar}
            />
            <AgendamentoSection
              title={t('clienteAgendamentos.status.awaitingConfirmation')}
              icon={<ClockCircleOutlined style={{ color: token.colorInfo, fontSize: 18 }} />}
              items={grupos.aguardandoConfirmacao}
              actionLoadingId={actionLoadingId}
              onCancel={handleCancelar}
              onOpenReschedule={setRemarcarItem}
              onOpenSchedule={setAgendarItem}
              onFollow={handleAcompanhar}
            />
            <AgendamentoSection
              title={t('clienteAgendamentos.status.scheduled')}
              icon={<CalendarOutlined style={{ color: token.colorPrimary, fontSize: 18 }} />}
              items={grupos.agendadas}
              actionLoadingId={actionLoadingId}
              onCancel={handleCancelar}
              onOpenReschedule={setRemarcarItem}
              onOpenSchedule={setAgendarItem}
              onFollow={handleAcompanhar}
            />
            <AgendamentoSection
              title={t('clienteAgendamentos.status.late')}
              icon={<ExclamationCircleOutlined style={{ color: token.colorError, fontSize: 18 }} />}
              items={grupos.atrasadas}
              actionLoadingId={actionLoadingId}
              onCancel={handleCancelar}
              onOpenReschedule={setRemarcarItem}
              onOpenSchedule={setAgendarItem}
              onFollow={handleAcompanhar}
            />
            <AgendamentoSection
              title={t('clienteAgendamentos.status.awaitingSchedule')}
              icon={<HourglassOutlined style={{ color: token.colorWarning, fontSize: 18 }} />}
              items={grupos.aguardandoAgendamento}
              actionLoadingId={actionLoadingId}
              onCancel={handleCancelar}
              onOpenReschedule={setRemarcarItem}
              onOpenSchedule={setAgendarItem}
              onFollow={handleAcompanhar}
            />
          </Flex>
        )}

        <RemarcarAgendamentoModal
          item={remarcarItem}
          open={Boolean(remarcarItem)}
          confirming={remarcando}
          onConfirm={handleRemarcar}
          onCancel={() => setRemarcarItem(null)}
        />
        <AgendamentoSolicitacaoModal
          item={agendarItem ? {
            marca: agendarItem.marca,
            modelo: agendarItem.modelo,
            numeroRevisao: agendarItem.numeroRevisao,
            dataMinima: agendarItem.dataMinima,
            dataLimite: agendarItem.dataLimite,
            dataIdeal: agendarItem.dataIdeal,
            dataAgendada: agendarItem.dataAgendada,
          } : null}
          lojas={lojas}
          open={Boolean(agendarItem)}
          confirming={agendando}
          mode={agendarItem?.status === STATUS.ATRASADA ? 'reagendar' : 'agendar'}
          onConfirm={handleAgendar}
          onCancel={() => setAgendarItem(null)}
        />
      </Flex>
    </Spin>
  );
}
