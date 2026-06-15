import { useEffect, useMemo, useState, type ReactNode } from 'react';
import {
  Alert,
  Button,
  Card,
  Col,
  Empty,
  Flex,
  Input,
  Result,
  Row,
  Select,
  Spin,
  Table,
  Tag,
  Typography,
  theme,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  CalendarOutlined,
  CarOutlined,
  CheckCircleFilled,
  ClockCircleFilled,
  CloseCircleFilled,
  DashboardOutlined,
  EyeOutlined,
  HourglassOutlined,
  SearchOutlined,
  ToolOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { getLocale, t } from '@/app/i18n';
import { Moto, RevisaoMotoResponse } from '@/app/models/Moto';
import { motoService } from '@/app/services/motoService';
import { PATHS } from '@/app/paths';
import { handleApiError } from '@/app/utils/errorHandler';
import { formatCurrency, formatIntegerInput } from '@/app/utils/formatters';
import {
  REVISION_STATUS,
  agruparRevisoesDasMotos,
  buildMotoRevisionDetailsPath,
  calcularResumoRevisoes,
  filtrarRevisoes,
  ordenarRevisoes,
} from '@/app/pages/cliente/revisoes/RevisoesCliente.utils';

const { Title, Text } = Typography;

type RevisionStatus = 'concluida' | 'em_execucao' | 'agendada' | 'atrasada' | 'planejada';
type ThemeToken = ReturnType<typeof theme.useToken>['token'];

interface RevisaoAgregada {
  key: string;
  motoId: number;
  moto: {
    id: number;
    placa: string;
    marca: string;
    nomeModelo: string;
    ano: number;
    cor: string;
    linha: string;
    kilometragemAtual: number;
    foto?: string;
  };
  revisao: RevisaoMotoResponse;
  status: RevisionStatus;
  dataPrevista: string;
  ordem: number;
  quilometragem: number;
  diasAteRevisao: number;
  totalPecas: number;
  totalServicos: number;
  totalEstimado: number;
  totalTempo: number;
}

const formatDate = (date: string) =>
  new Date(`${date.substring(0, 10)}T00:00:00`).toLocaleDateString(getLocale());

const getRevisionTitle = (revisao: RevisaoMotoResponse) =>
  t('motoDetalhes.revisions.order', { order: revisao.ordem });

const getStatusConfig = (status: RevisionStatus, token: ThemeToken) => {
  const configs = {
    concluida: {
      label: t('motoDetalhes.status.concluida'),
      tagColor: 'success',
      cardColor: token.colorSuccess,
      icon: <CheckCircleFilled style={{ color: token.colorSuccess }} />,
    },
    em_execucao: {
      label: t('motoDetalhes.status.emExecucao'),
      tagColor: 'processing',
      cardColor: token.colorInfo,
      icon: <ClockCircleFilled style={{ color: token.colorInfo }} />,
    },
    agendada: {
      label: t('motoDetalhes.status.agendada'),
      tagColor: 'blue',
      cardColor: token.colorInfo,
      icon: <CalendarOutlined style={{ color: token.colorInfo }} />,
    },
    atrasada: {
      label: t('motoDetalhes.status.atrasada'),
      tagColor: 'error',
      cardColor: token.colorError,
      icon: <CloseCircleFilled style={{ color: token.colorError }} />,
    },
    planejada: {
      label: t('motoDetalhes.status.planejada'),
      tagColor: 'default',
      cardColor: token.colorTextSecondary,
      icon: <HourglassOutlined style={{ color: token.colorTextTertiary }} />,
    },
  };

  return configs[status];
};

function getDeadlineText(item: RevisaoAgregada) {
  if (item.status === REVISION_STATUS.CONCLUIDA) return t('clienteRevisoes.deadline.completed');
  if (item.diasAteRevisao < 0) {
    return t('clienteRevisoes.deadline.overdue', { days: Math.abs(item.diasAteRevisao) });
  }
  if (item.diasAteRevisao === 0) return t('clienteRevisoes.deadline.today');
  return t('clienteRevisoes.deadline.remaining', { days: item.diasAteRevisao });
}

function SummaryCard({
  title,
  value,
  color,
  icon,
}: {
  title: string;
  value: number;
  color: string;
  icon: ReactNode;
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
          }}
        >
          {icon}
        </Flex>
      </Flex>
    </Card>
  );
}

export default function RevisoesCliente() {
  const { token } = theme.useToken();
  const navigate = useNavigate();
  const [motos, setMotos] = useState<Moto[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState(false);
  const [busca, setBusca] = useState('');
  const [motoFiltro, setMotoFiltro] = useState<number | 'todas'>('todas');
  const [statusFiltro, setStatusFiltro] = useState<RevisionStatus | 'todos'>('todos');

  useEffect(() => {
    const carregarMotos = async () => {
      try {
        setLoading(true);
        setLoadError(false);
        const motosData = await motoService.getAll();
        setMotos(motosData);
      } catch (error) {
        setLoadError(true);
        handleApiError(error);
      } finally {
        setLoading(false);
      }
    };

    carregarMotos();
  }, []);

  const revisoes = useMemo<RevisaoAgregada[]>(
    () => ordenarRevisoes(agruparRevisoesDasMotos(motos)) as RevisaoAgregada[],
    [motos]
  );

  const handleOpenDetails = (item: RevisaoAgregada) => {
    navigate(buildMotoRevisionDetailsPath(
      PATHS.CLIENTE_MOTOS_DETALHES,
      item.motoId,
      item.revisao.id,
      item.status,
      PATHS.CLIENTE_REVISOES
    ));
  };

  const revisoesFiltradas = useMemo<RevisaoAgregada[]>(
    () => filtrarRevisoes(revisoes, { motoId: motoFiltro, status: statusFiltro, busca }) as RevisaoAgregada[],
    [busca, motoFiltro, revisoes, statusFiltro]
  );

  const resumo = useMemo(() => calcularResumoRevisoes(revisoes), [revisoes]);

  const statusOptions = [
    { value: 'todos', label: t('clienteRevisoes.filters.allStatuses') },
    { value: REVISION_STATUS.ATRASADA, label: t('motoDetalhes.status.atrasada') },
    { value: REVISION_STATUS.EM_EXECUCAO, label: t('motoDetalhes.status.emExecucao') },
    { value: REVISION_STATUS.AGENDADA, label: t('motoDetalhes.status.agendada') },
    { value: REVISION_STATUS.PLANEJADA, label: t('motoDetalhes.status.planejada') },
    { value: REVISION_STATUS.CONCLUIDA, label: t('motoDetalhes.status.concluida') },
  ];

  const columns: ColumnsType<RevisaoAgregada> = [
    {
      title: t('clienteRevisoes.table.motorcycle'),
      key: 'moto',
      minWidth: 220,
      render: (_, item) => (
        <Flex vertical gap={2}>
          <Text strong>{item.moto.marca} {item.moto.nomeModelo}</Text>
          <Text type="secondary" style={{ fontSize: 12 }}>
            {item.moto.placa} - {item.moto.ano}
          </Text>
        </Flex>
      ),
    },
    {
      title: t('clienteRevisoes.table.revision'),
      key: 'revisao',
      minWidth: 180,
      render: (_, item) => (
        <Flex vertical gap={2}>
          <Text>{item.revisao.nome}</Text>
          <Text type="secondary" style={{ fontSize: 12 }}>{getRevisionTitle(item.revisao)}</Text>
        </Flex>
      ),
    },
    {
      title: t('clienteRevisoes.table.status'),
      key: 'status',
      width: 150,
      render: (_, item) => {
        const config = getStatusConfig(item.status, token);
        return <Tag icon={config.icon} color={config.tagColor}>{config.label}</Tag>;
      },
    },
    {
      title: t('clienteRevisoes.table.mileage'),
      key: 'quilometragem',
      width: 140,
      render: (_, item) => <Text>{formatIntegerInput(item.quilometragem)} km</Text>,
    },
    {
      title: t('clienteRevisoes.table.idealDate'),
      key: 'dataPrevista',
      width: 130,
      render: (_, item) => <Text>{formatDate(item.dataPrevista)}</Text>,
    },
    {
      title: t('clienteRevisoes.table.deadline'),
      key: 'deadline',
      minWidth: 160,
      render: (_, item) => (
        <Text type={item.status === REVISION_STATUS.ATRASADA ? 'danger' : 'secondary'}>
          {getDeadlineText(item)}
        </Text>
      ),
    },
    {
      title: t('clienteRevisoes.table.estimate'),
      key: 'estimate',
      width: 130,
      align: 'right',
      render: (_, item) => <Text strong>{formatCurrency(item.totalEstimado)}</Text>,
    },
    {
      title: '',
      key: 'actions',
      width: 130,
      render: (_, item) => (
        <Button
          size="small"
          icon={<EyeOutlined />}
          onClick={() => handleOpenDetails(item)}
        >
          {t('clienteRevisoes.actions.details')}
        </Button>
      ),
    },
  ];

  if (loadError && !loading) {
    return (
      <Flex vertical justify="center" align="center" style={{ width: '100%', minHeight: '60vh' }}>
        <Result
          status="500"
          title={t('clienteRevisoes.error.title')}
          subTitle={t('clienteRevisoes.error.message')}
          extra={
            <Button type="primary" onClick={() => window.location.reload()}>
              {t('clienteRevisoes.error.reload')}
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
              { title: t('clienteRevisoes.title'), path: PATHS.CLIENTE_REVISOES, icon: <ToolOutlined /> },
            ]}
          />

          <Flex justify="space-between" align="flex-start" gap="middle" wrap="wrap">
            <Flex vertical gap={4}>
              <Title level={2} style={{ margin: 0 }}>{t('clienteRevisoes.title')}</Title>
              <Text type="secondary">{t('clienteRevisoes.subtitle')}</Text>
            </Flex>
          </Flex>
        </Flex>

        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} lg={4}>
            <SummaryCard
              title={t('clienteRevisoes.summary.total')}
              value={resumo.total}
              color={token.colorText}
              icon={<ToolOutlined style={{ color: token.colorTextSecondary }} />}
            />
          </Col>
          <Col xs={24} sm={12} lg={4}>
            <SummaryCard
              title={t('clienteRevisoes.summary.overdue')}
              value={resumo.atrasadas}
              color={token.colorError}
              icon={<CloseCircleFilled style={{ color: token.colorError }} />}
            />
          </Col>
          <Col xs={24} sm={12} lg={4}>
            <SummaryCard
              title={t('clienteRevisoes.summary.scheduled')}
              value={resumo.agendadas}
              color={token.colorInfo}
              icon={<CalendarOutlined style={{ color: token.colorInfo }} />}
            />
          </Col>
          <Col xs={24} sm={12} lg={4}>
            <SummaryCard
              title={t('clienteRevisoes.summary.inProgress')}
              value={resumo.emExecucao}
              color={token.colorInfo}
              icon={<ClockCircleFilled style={{ color: token.colorInfo }} />}
            />
          </Col>
          <Col xs={24} sm={12} lg={4}>
            <SummaryCard
              title={t('clienteRevisoes.summary.pending')}
              value={resumo.pendentes}
              color={token.colorTextSecondary}
              icon={<HourglassOutlined style={{ color: token.colorTextSecondary }} />}
            />
          </Col>
          <Col xs={24} sm={12} lg={4}>
            <SummaryCard
              title={t('clienteRevisoes.summary.completed')}
              value={resumo.concluidas}
              color={token.colorSuccess}
              icon={<CheckCircleFilled style={{ color: token.colorSuccess }} />}
            />
          </Col>
        </Row>

        {resumo.atrasadas > 0 && (
          <Alert
            type="error"
            showIcon
            message={t('clienteRevisoes.alert.overdue.title', { count: resumo.atrasadas })}
            description={t('clienteRevisoes.alert.overdue.description')}
          />
        )}

        <Card>
          <Flex vertical gap="middle">
            <Flex gap="middle" wrap="wrap" align="center">
              <Input
                allowClear
                value={busca}
                onChange={(event) => setBusca(event.target.value)}
                placeholder={t('clienteRevisoes.filters.search.placeholder')}
                prefix={<SearchOutlined />}
                style={{ maxWidth: 360 }}
              />

              <Select
                value={motoFiltro}
                onChange={setMotoFiltro}
                style={{ width: 260 }}
                options={[
                  { value: 'todas', label: t('clienteRevisoes.filters.allMotorcycles') },
                  ...motos.map((moto) => ({
                    value: moto.id,
                    label: `${moto.marca} ${moto.nomeModelo} - ${moto.placa}`,
                  })),
                ]}
              />

              <Select
                value={statusFiltro}
                onChange={setStatusFiltro}
                style={{ width: 220 }}
                options={statusOptions}
              />
            </Flex>

            <Text type="secondary">
              {t('clienteRevisoes.found', { count: revisoesFiltradas.length })}
            </Text>

            {revisoes.length === 0 ? (
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description={t('clienteRevisoes.empty.noRevisions')}
              >
                <Button type="primary" href={PATHS.CLIENTE_MOTOS}>
                  <CarOutlined />
                  {t('clienteRevisoes.empty.goToMotorcycles')}
                </Button>
              </Empty>
            ) : (
              <Table
                dataSource={revisoesFiltradas}
                columns={columns}
                rowKey="key"
                pagination={{ pageSize: 10, showSizeChanger: false }}
                size="middle"
                scroll={{ x: true }}
                locale={{ emptyText: t('clienteRevisoes.empty.filtered') }}
              />
            )}
          </Flex>
        </Card>
      </Flex>
    </Spin>
  );
}

