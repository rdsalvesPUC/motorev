import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import {
  Avatar,
  Badge,
  Card,
  Col,
  Empty,
  Flex,
  Progress,
  Result,
  Row,
  Spin,
  Statistic,
  Tag,
  Typography,
  theme,
} from 'antd';
import {
  CalendarOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  SyncOutlined,
  ToolOutlined,
  UserOutlined,
} from '@ant-design/icons';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { getLocale, t } from '@/app/i18n';
import {
  DashboardConcessionariaRevisoes,
  FilaMecanico,
  ItemFilaRevisao,
  StatusFilaRevisao,
} from '@/app/models/DashboardConcessionaria';
import { dashboardService } from '@/app/services/dashboardService';
import { handleApiError } from '@/app/utils/errorHandler';

const { Title, Text } = Typography;

type ThemeToken = ReturnType<typeof theme.useToken>['token'];

const formatDateTime = (date?: string | null) => {
  if (!date) return '-';
  return new Date(date).toLocaleString(getLocale(), {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
};

const formatDashboardDate = (date?: string | null) => {
  if (!date) return '-';
  return new Date(`${date.substring(0, 10)}T00:00:00`).toLocaleDateString(getLocale(), {
    weekday: 'long',
    day: '2-digit',
    month: 'long',
    year: 'numeric',
  });
};

const getStatusConfig = (status: StatusFilaRevisao, token: ThemeToken) => {
  const configs: Record<StatusFilaRevisao, { label: string; badge: 'default' | 'processing' | 'success'; tag: string; color: string }> = {
    agendada: {
      label: t('concessionariaDashboard.status.scheduled'),
      badge: 'default',
      tag: 'blue',
      color: token.colorPrimary,
    },
    em_execucao: {
      label: t('concessionariaDashboard.status.inProgress'),
      badge: 'processing',
      tag: 'cyan',
      color: token.colorInfo,
    },
    concluida: {
      label: t('concessionariaDashboard.status.completed'),
      badge: 'success',
      tag: 'success',
      color: token.colorSuccess,
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
        <Statistic
          title={title}
          value={value}
          valueStyle={{ color, fontSize: 24, fontWeight: 700 }}
        />
        <Flex
          align="center"
          justify="center"
          style={{
            width: 44,
            height: 44,
            borderRadius: token.borderRadiusLG,
            background: token.colorFillSecondary,
            color,
            fontSize: 20,
          }}
        >
          {icon}
        </Flex>
      </Flex>
    </Card>
  );
}

function CardRevisaoFila({ item }: { item: ItemFilaRevisao }) {
  const { token } = theme.useToken();
  const status = getStatusConfig(item.status, token);
  const isEmExecucao = item.status === 'em_execucao';

  return (
    <Card
      size="small"
      style={{
        borderColor: isEmExecucao ? token.colorInfo : token.colorBorderSecondary,
        background: isEmExecucao ? token.colorInfoBg : token.colorBgContainer,
      }}
      styles={{ body: { padding: 14 } }}
    >
      <Flex vertical gap={10}>
        <Flex justify="space-between" align="center" gap="small">
          <Badge
            status={status.badge}
            text={item.status === 'agendada'
              ? t('concessionariaDashboard.queue.position', { position: item.posicaoFila })
              : status.label}
          />
          <Tag color={status.tag}>{status.label}</Tag>
        </Flex>

        <Flex vertical gap={2}>
          <Text strong style={{ fontSize: 15 }}>
            {item.marca} {item.modelo}
          </Text>
          <Text type="secondary" style={{ fontSize: 13 }}>
            {item.clienteNome} - {item.placa}
          </Text>
          <Text type="secondary" style={{ fontSize: 12 }}>
            {t('concessionariaDashboard.card.revision', { number: item.numeroRevisao })} - {item.quilometragem.toLocaleString(getLocale())} km
          </Text>
          <Text type="secondary" style={{ fontSize: 12 }}>
            {item.lojaNome} - {formatDateTime(item.dataAgendada)}
          </Text>
        </Flex>

        {isEmExecucao || item.status === 'concluida' ? (
          <Flex vertical gap={4}>
            <Text style={{ fontSize: 12 }}>
              {t('concessionariaDashboard.card.progressItems', {
                completed: item.itensConcluidos,
                total: item.totalItens,
              })}
            </Text>
            <Progress
              percent={item.progressoPercentual}
              size="small"
              status={item.status === 'concluida' ? 'success' : 'active'}
            />
          </Flex>
        ) : (
          <Text type="secondary" style={{ fontSize: 12 }}>
            {t('concessionariaDashboard.card.waitingTurn')}
          </Text>
        )}
      </Flex>
    </Card>
  );
}

function ColunaMecanico({ fila }: { fila: FilaMecanico }) {
  const { token } = theme.useToken();

  return (
    <Card
      title={
        <Flex vertical align="center" gap={8} style={{ padding: '8px 0' }}>
          <Avatar size={58} icon={<UserOutlined />} style={{ backgroundColor: token.colorPrimary }} />
          <Text strong style={{ fontSize: 16, textAlign: 'center' }}>{fila.nome}</Text>
          <Flex gap={6} wrap="wrap" justify="center">
            <Tag color="blue">{fila.especialidade}</Tag>
            <Tag color={fila.itens.length > 0 ? 'orange' : 'default'}>
              {t('concessionariaDashboard.mechanic.queueCount', { count: fila.itens.length })}
            </Tag>
          </Flex>
        </Flex>
      }
      style={{ height: '100%' }}
      styles={{ body: { padding: 16 } }}
    >
      <Flex vertical gap={12}>
        {fila.itens.length === 0 ? (
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description={t('concessionariaDashboard.empty.queue')}
          />
        ) : (
          fila.itens.map((item) => (
            <CardRevisaoFila key={item.agendamentoId} item={item} />
          ))
        )}
      </Flex>
    </Card>
  );
}

export default function DashboardRevisoesConcessionaria() {
  const { token } = theme.useToken();
  const [dashboard, setDashboard] = useState<DashboardConcessionariaRevisoes | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState(false);
  const [now, setNow] = useState(() => new Date());

  const loadDashboard = useCallback(async (showLoading = true) => {
    if (showLoading) {
      setLoading(true);
    }
    setLoadError(false);

    try {
      const data = await dashboardService.getConcessionariaRevisoes();
      setDashboard(data);
    } catch (error) {
      setLoadError(true);
      handleApiError(error, t('concessionariaDashboard.load.error'));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadDashboard();
  }, [loadDashboard]);

  useEffect(() => {
    const interval = window.setInterval(() => {
      setNow(new Date());
      loadDashboard(false);
    }, 30000);

    return () => window.clearInterval(interval);
  }, [loadDashboard]);

  const horaAtual = useMemo(() => (
    now.toLocaleTimeString(getLocale(), { hour: '2-digit', minute: '2-digit' })
  ), [now]);

  if (loading) {
    return (
      <Flex justify="center" align="center" style={{ minHeight: 360 }}>
        <Spin size="large" />
      </Flex>
    );
  }

  if (loadError || !dashboard) {
    return (
      <Result
        status="500"
        title={t('concessionariaDashboard.error.title')}
        subTitle={t('concessionariaDashboard.error.message')}
      />
    );
  }

  return (
    <Flex vertical gap="large" style={{ width: '100%' }}>
      <DashboardBreadcrumb
        userType="concessionaria"
        items={[{ title: t('dashboard.menu.dashboard'), icon: <ToolOutlined /> }]}
      />

      <Card
        style={{
          background: `linear-gradient(135deg, ${token.colorPrimaryBg} 0%, ${token.colorBgContainer} 62%)`,
          borderColor: token.colorBorderSecondary,
        }}
        styles={{ body: { padding: 24 } }}
      >
        <Flex justify="space-between" align="center" wrap="wrap" gap="middle">
          <Flex vertical gap={6}>
            <Title level={2} style={{ margin: 0 }}>
              {t('concessionariaDashboard.title')}
            </Title>
            <Text type="secondary">
              {formatDashboardDate(dashboard.data)} - {t('concessionariaDashboard.header.updatedAt', { time: horaAtual })}
            </Text>
          </Flex>
          <Tag color="processing" style={{ padding: '6px 10px', borderRadius: token.borderRadiusLG }}>
            <SyncOutlined spin /> {t('concessionariaDashboard.header.autoRefresh')}
          </Tag>
        </Flex>
      </Card>

      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <SummaryCard
            title={t('concessionariaDashboard.stats.mechanics')}
            value={dashboard.totalMecanicos}
            color={token.colorPrimary}
            icon={<ToolOutlined />}
          />
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <SummaryCard
            title={t('concessionariaDashboard.stats.today')}
            value={dashboard.totalRevisoes}
            color={token.colorWarning}
            icon={<CalendarOutlined />}
          />
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <SummaryCard
            title={t('concessionariaDashboard.stats.inProgress')}
            value={dashboard.revisoesEmExecucao}
            color={token.colorInfo}
            icon={<ClockCircleOutlined />}
          />
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <SummaryCard
            title={t('concessionariaDashboard.stats.completed')}
            value={dashboard.revisoesConcluidas}
            color={token.colorSuccess}
            icon={<CheckCircleOutlined />}
          />
        </Col>
      </Row>

      {dashboard.totalRevisoes === 0 ? (
        <Card>
          <Empty description={t('concessionariaDashboard.empty.day')} />
        </Card>
      ) : (
        <Row gutter={[16, 16]}>
          {dashboard.filas.map((fila) => (
            <Col key={fila.mecanicoId} xs={24} md={12} xl={6}>
              <ColunaMecanico fila={fila} />
            </Col>
          ))}
        </Row>
      )}
    </Flex>
  );
}
