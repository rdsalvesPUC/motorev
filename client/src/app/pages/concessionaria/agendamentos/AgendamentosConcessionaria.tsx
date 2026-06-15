import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import {
  Button,
  Card,
  Empty,
  Flex,
  Form,
  Input,
  message,
  Modal,
  Popconfirm,
  Result,
  Row,
  Col,
  Spin,
  Table,
  Tabs,
  Tag,
  Typography,
  theme,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  CalendarOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  CloseCircleOutlined,
  ExclamationCircleOutlined,
  ShopOutlined,
  ToolOutlined,
} from '@ant-design/icons';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { getLocale, t } from '@/app/i18n';
import {
  AgendamentoConcessionaria,
  StatusAgendamentoConcessionaria,
} from '@/app/models/AgendamentoConcessionaria';
import { PATHS } from '@/app/paths';
import { agendamentoService } from '@/app/services/agendamentoService';
import { handleApiError } from '@/app/utils/errorHandler';

const { Title, Text } = Typography;

type ThemeToken = ReturnType<typeof theme.useToken>['token'];

const STATUS = {
  AGUARDANDO_CONFIRMACAO: 'aguardando_confirmacao',
  AGENDADA: 'agendada',
  RECUSADA: 'recusada',
  EM_EXECUCAO: 'em_execucao',
  CONCLUIDA: 'concluida',
} as const satisfies Record<string, StatusAgendamentoConcessionaria>;

const formatDate = (date?: string | null) => {
  if (!date) return '-';
  return new Date(`${date.substring(0, 10)}T00:00:00`).toLocaleDateString(getLocale());
};

const getStatusConfig = (status: StatusAgendamentoConcessionaria, token: ThemeToken) => {
  const configs: Record<StatusAgendamentoConcessionaria, { label: string; color: string; icon: ReactNode; accent: string }> = {
    aguardando_confirmacao: {
      label: t('concessionariaAgendamentos.status.awaitingConfirmation'),
      color: 'processing',
      icon: <ClockCircleOutlined />,
      accent: token.colorInfo,
    },
    agendada: {
      label: t('concessionariaAgendamentos.status.scheduled'),
      color: 'blue',
      icon: <CalendarOutlined />,
      accent: token.colorPrimary,
    },
    recusada: {
      label: t('concessionariaAgendamentos.status.refused'),
      color: 'error',
      icon: <CloseCircleOutlined />,
      accent: token.colorError,
    },
    em_execucao: {
      label: t('concessionariaAgendamentos.status.inProgress'),
      color: 'cyan',
      icon: <ToolOutlined />,
      accent: token.colorInfo,
    },
    concluida: {
      label: t('concessionariaAgendamentos.status.completed'),
      color: 'success',
      icon: <CheckCircleOutlined />,
      accent: token.colorSuccess,
    },
    cancelada: {
      label: t('concessionariaAgendamentos.status.cancelled'),
      color: 'default',
      icon: <CloseCircleOutlined />,
      accent: token.colorTextTertiary,
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

function RecusarAgendamentoModal({
  item,
  open,
  confirming,
  onConfirm,
  onCancel,
}: {
  item: AgendamentoConcessionaria | null;
  open: boolean;
  confirming: boolean;
  onConfirm: (motivo?: string) => Promise<void>;
  onCancel: () => void;
}) {
  const [form] = Form.useForm<{ motivo?: string }>();

  useEffect(() => {
    if (!open) {
      form.resetFields();
    }
  }, [form, open]);

  const handleOk = async () => {
    const values = await form.validateFields();
    await onConfirm(values.motivo);
  };

  return (
    <Modal
      title={t('concessionariaAgendamentos.refuseModal.title')}
      open={open}
      onOk={handleOk}
      onCancel={onCancel}
      okText={t('concessionariaAgendamentos.refuseModal.ok')}
      cancelText={t('concessionariaAgendamentos.refuseModal.cancel')}
      okButtonProps={{ danger: true }}
      confirmLoading={confirming}
      destroyOnHidden
    >
      <Flex vertical gap="middle">
        {item && (
          <Card size="small">
            <Flex vertical gap={4}>
              <Text strong>{item.clienteNome}</Text>
              <Text type="secondary">
                {item.marca} {item.modelo} - {t('concessionariaAgendamentos.table.revisionValue', { number: item.numeroRevisao })}
              </Text>
              <Text type="secondary">
                {t('concessionariaAgendamentos.table.requestedDate')}: {formatDate(item.dataAgendada)}
              </Text>
            </Flex>
          </Card>
        )}

        <Form form={form} layout="vertical">
          <Form.Item
            name="motivo"
            label={t('concessionariaAgendamentos.refuseModal.reason.label')}
            rules={[{ max: 500, message: t('concessionariaAgendamentos.refuseModal.reason.max') }]}
          >
            <Input.TextArea
              rows={4}
              maxLength={500}
              showCount
              placeholder={t('concessionariaAgendamentos.refuseModal.reason.placeholder')}
            />
          </Form.Item>
        </Form>

        <Text type="secondary">{t('concessionariaAgendamentos.refuseModal.footer')}</Text>
      </Flex>
    </Modal>
  );
}

export default function AgendamentosConcessionaria() {
  const { token } = theme.useToken();
  const [agendamentos, setAgendamentos] = useState<AgendamentoConcessionaria[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState(false);
  const [actionLoadingId, setActionLoadingId] = useState<number | null>(null);
  const [recusarItem, setRecusarItem] = useState<AgendamentoConcessionaria | null>(null);
  const [recusando, setRecusando] = useState(false);

  const carregarAgendamentos = useCallback(async (showLoading = true) => {
    try {
      if (showLoading) {
        setLoading(true);
      }
      setLoadError(false);
      const data = await agendamentoService.getConcessionaria();
      setAgendamentos(data);
    } catch (error) {
      setLoadError(true);
      handleApiError(error, 'concessionariaAgendamentos.load.error');
    } finally {
      if (showLoading) {
        setLoading(false);
      }
    }
  }, []);

  useEffect(() => {
    carregarAgendamentos();
  }, [carregarAgendamentos]);

  const grupos = useMemo(() => ({
    solicitacoes: agendamentos.filter((item) => item.status === STATUS.AGUARDANDO_CONFIRMACAO),
    agendadas: agendamentos.filter((item) => item.status === STATUS.AGENDADA),
    recusadas: agendamentos.filter((item) => item.status === STATUS.RECUSADA),
    emAndamento: agendamentos.filter((item) => item.status === STATUS.EM_EXECUCAO),
    concluidas: agendamentos.filter((item) => item.status === STATUS.CONCLUIDA),
  }), [agendamentos]);

  const handleAceitar = async (item: AgendamentoConcessionaria) => {
    try {
      setActionLoadingId(item.agendamentoId);
      await agendamentoService.aceitarConcessionaria(item.agendamentoId);
      message.success(t('concessionariaAgendamentos.actions.accept.success'));
      await carregarAgendamentos(false);
    } catch (error) {
      handleApiError(error, 'concessionariaAgendamentos.actions.accept.error');
    } finally {
      setActionLoadingId(null);
    }
  };

  const handleRecusar = async (motivo?: string) => {
    if (!recusarItem) return;

    try {
      setRecusando(true);
      setActionLoadingId(recusarItem.agendamentoId);
      await agendamentoService.recusarConcessionaria(recusarItem.agendamentoId, motivo);
      message.success(t('concessionariaAgendamentos.actions.refuse.success'));
      setRecusarItem(null);
      await carregarAgendamentos(false);
    } catch (error) {
      handleApiError(error, 'concessionariaAgendamentos.actions.refuse.error');
    } finally {
      setRecusando(false);
      setActionLoadingId(null);
    }
  };

  const baseColumns: ColumnsType<AgendamentoConcessionaria> = [
    {
      title: t('concessionariaAgendamentos.table.client'),
      dataIndex: 'clienteNome',
      key: 'clienteNome',
      render: (_, record) => (
        <Flex vertical gap={2}>
          <Text strong>{record.clienteNome}</Text>
          <Text type="secondary" style={{ fontSize: 12 }}>{record.placa}</Text>
        </Flex>
      ),
    },
    {
      title: t('concessionariaAgendamentos.table.motorcycle'),
      key: 'moto',
      render: (_, record) => (
        <Text>{record.marca} {record.modelo}</Text>
      ),
    },
    {
      title: t('concessionariaAgendamentos.table.revision'),
      key: 'revision',
      render: (_, record) => (
        <Flex vertical gap={2}>
          <Text>{t('concessionariaAgendamentos.table.revisionValue', { number: record.numeroRevisao })}</Text>
          <Text type="secondary" style={{ fontSize: 12 }}>{record.nomeRevisao}</Text>
        </Flex>
      ),
    },
    {
      title: t('concessionariaAgendamentos.table.shop'),
      dataIndex: 'lojaNome',
      key: 'lojaNome',
      render: (lojaNome: string) => (
        <Flex align="center" gap={6}>
          <ShopOutlined style={{ color: token.colorTextTertiary }} />
          <span>{lojaNome}</span>
        </Flex>
      ),
    },
    {
      title: t('concessionariaAgendamentos.table.idealDate'),
      dataIndex: 'dataIdeal',
      key: 'dataIdeal',
      render: formatDate,
    },
    {
      title: t('concessionariaAgendamentos.table.requestedDate'),
      dataIndex: 'dataAgendada',
      key: 'dataAgendada',
      render: formatDate,
    },
    {
      title: t('concessionariaAgendamentos.table.items'),
      key: 'items',
      render: (_, record) => t('concessionariaAgendamentos.table.itemsValue', {
        parts: record.quantidadePecas,
        services: record.quantidadeServicos,
      }),
    },
    {
      title: t('concessionariaAgendamentos.table.status'),
      dataIndex: 'status',
      key: 'status',
      render: (status: StatusAgendamentoConcessionaria) => {
        const config = getStatusConfig(status, token);
        return (
          <Tag icon={config.icon} color={config.color} style={{ marginInlineEnd: 0 }}>
            {config.label}
          </Tag>
        );
      },
    },
  ];

  const solicitacaoColumns: ColumnsType<AgendamentoConcessionaria> = [
    ...baseColumns,
    {
      title: t('concessionariaAgendamentos.table.actions'),
      key: 'actions',
      fixed: 'right',
      render: (_, record) => {
        const loadingAction = actionLoadingId === record.agendamentoId;
        return (
          <Flex gap="small" wrap="wrap">
            <Popconfirm
              title={t('concessionariaAgendamentos.acceptConfirm.title')}
              description={t('concessionariaAgendamentos.acceptConfirm.description')}
              okText={t('concessionariaAgendamentos.acceptConfirm.ok')}
              cancelText={t('concessionariaAgendamentos.acceptConfirm.cancel')}
              onConfirm={() => handleAceitar(record)}
              okButtonProps={{ loading: loadingAction }}
            >
              <Button type="primary" size="small" loading={loadingAction}>
                {t('concessionariaAgendamentos.actions.accept')}
              </Button>
            </Popconfirm>
            <Button
              danger
              size="small"
              disabled={loadingAction}
              onClick={() => setRecusarItem(record)}
            >
              {t('concessionariaAgendamentos.actions.refuse')}
            </Button>
          </Flex>
        );
      },
    },
  ];

  const refusedColumns: ColumnsType<AgendamentoConcessionaria> = [
    ...baseColumns,
    {
      title: t('concessionariaAgendamentos.table.refuseReason'),
      dataIndex: 'mensagemRecusa',
      key: 'mensagemRecusa',
      render: (mensagem?: string | null) => mensagem || '-',
    },
  ];

  const renderTable = (
    items: AgendamentoConcessionaria[],
    columns: ColumnsType<AgendamentoConcessionaria> = baseColumns,
  ) => (
    <Table
      rowKey="agendamentoId"
      columns={columns}
      dataSource={items}
      pagination={{ pageSize: 8, showSizeChanger: false }}
      locale={{ emptyText: <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t('concessionariaAgendamentos.empty.tab')} /> }}
      scroll={{ x: 1100 }}
    />
  );

  if (loadError && !loading) {
    return (
      <Flex vertical justify="center" align="center" style={{ width: '100%', minHeight: '60vh' }}>
        <Result
          status="500"
          title={t('concessionariaAgendamentos.error.title')}
          subTitle={t('concessionariaAgendamentos.error.message')}
          extra={
            <Button type="primary" onClick={() => window.location.reload()}>
              {t('concessionariaAgendamentos.error.reload')}
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
            userType="concessionaria"
            items={[
              {
                title: t('concessionariaAgendamentos.title'),
                path: PATHS.CONCESSIONARIA_AGENDAMENTOS,
                icon: <CalendarOutlined />,
              },
            ]}
          />

          <Flex vertical gap={4}>
            <Title level={2} style={{ margin: 0 }}>{t('concessionariaAgendamentos.title')}</Title>
            <Text type="secondary">{t('concessionariaAgendamentos.subtitle')}</Text>
          </Flex>
        </Flex>

        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} lg={6}>
            <SummaryCard
              title={t('concessionariaAgendamentos.summary.requests')}
              value={grupos.solicitacoes.length}
              color={token.colorInfo}
              icon={<ClockCircleOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <SummaryCard
              title={t('concessionariaAgendamentos.summary.scheduled')}
              value={grupos.agendadas.length}
              color={token.colorPrimary}
              icon={<CalendarOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <SummaryCard
              title={t('concessionariaAgendamentos.summary.inProgress')}
              value={grupos.emAndamento.length}
              color={token.colorInfo}
              icon={<ToolOutlined />}
            />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <SummaryCard
              title={t('concessionariaAgendamentos.summary.completed')}
              value={grupos.concluidas.length}
              color={token.colorSuccess}
              icon={<CheckCircleOutlined />}
            />
          </Col>
        </Row>

        <Card>
          <Tabs
            items={[
              {
                key: STATUS.AGUARDANDO_CONFIRMACAO,
                label: t('concessionariaAgendamentos.tabs.requests', { count: grupos.solicitacoes.length }),
                children: renderTable(grupos.solicitacoes, solicitacaoColumns),
              },
              {
                key: STATUS.AGENDADA,
                label: t('concessionariaAgendamentos.tabs.scheduled', { count: grupos.agendadas.length }),
                children: renderTable(grupos.agendadas),
              },
              {
                key: STATUS.RECUSADA,
                label: t('concessionariaAgendamentos.tabs.refused', { count: grupos.recusadas.length }),
                children: renderTable(grupos.recusadas, refusedColumns),
              },
              {
                key: STATUS.EM_EXECUCAO,
                label: t('concessionariaAgendamentos.tabs.inProgress', { count: grupos.emAndamento.length }),
                children: renderTable(grupos.emAndamento),
              },
              {
                key: STATUS.CONCLUIDA,
                label: t('concessionariaAgendamentos.tabs.completed', { count: grupos.concluidas.length }),
                children: renderTable(grupos.concluidas),
              },
            ]}
          />
        </Card>

        <RecusarAgendamentoModal
          item={recusarItem}
          open={Boolean(recusarItem)}
          confirming={recusando}
          onConfirm={handleRecusar}
          onCancel={() => setRecusarItem(null)}
        />
      </Flex>
    </Spin>
  );
}
