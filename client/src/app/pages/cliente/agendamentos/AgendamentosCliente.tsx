import { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  Card,
  Col,
  DatePicker,
  Descriptions,
  Drawer,
  Empty,
  Flex,
  Form,
  message,
  Modal,
  Popconfirm,
  Row,
  Select,
  Spin,
  Statistic,
  Tag,
  Typography,
} from 'antd';
import {
  CalendarOutlined,
  CarOutlined,
  ClockCircleOutlined,
  CloseCircleOutlined,
  EnvironmentOutlined,
  EyeOutlined,
  HourglassOutlined,
  SyncOutlined,
  ToolOutlined,
  WarningOutlined,
} from '@ant-design/icons';
import dayjs, { type Dayjs } from 'dayjs';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { Moto, RevisaoMotoResponse } from '@/app/models/Moto';
import { Loja } from '@/app/models/Loja';
import { motoService } from '@/app/services/motoService';
import { concessionariaService } from '@/app/services/concessionariaService';
import { handleApiError } from '@/app/utils/errorHandler';
import { formatCurrency, formatIntegerInput } from '@/app/utils/formatters';
import { getLocale } from '@/app/i18n';
import {
  AGENDAMENTO_STATUS,
  buildAgendamentoItems,
} from '@/app/pages/cliente/agendamentos/AgendamentosCliente.utils';

const { Title, Text } = Typography;

type AgendamentoStatus =
  | 'aguardando_agendamento'
  | 'aguardando_confirmacao'
  | 'agendada'
  | 'em_execucao'
  | 'atrasada'
  | 'perdida';

interface AgendamentoItem {
  key: string;
  moto: Moto;
  revisao: RevisaoMotoResponse;
  status: AgendamentoStatus;
  dataIdeal: Date;
  dataMinima: Date;
  dataLimite: Date;
  prazoTexto: string;
}

interface AgendamentoModalProps {
  item: AgendamentoItem | null;
  lojas: Loja[];
  title: string;
  okText: string;
  open: boolean;
  submitting: boolean;
  onCancel: () => void;
  onConfirm: (lojaId: number, dataAgendamento: string) => Promise<void>;
}

const statusConfig: Record<AgendamentoStatus, { label: string; color: string; icon: React.ReactNode }> = {
  aguardando_agendamento: {
    label: 'Aguardando Agendamento',
    color: 'gold',
    icon: <ClockCircleOutlined />,
  },
  aguardando_confirmacao: {
    label: 'Aguardando Confirmação',
    color: 'orange',
    icon: <HourglassOutlined />,
  },
  agendada: {
    label: 'Agendada',
    color: 'blue',
    icon: <CalendarOutlined />,
  },
  em_execucao: {
    label: 'Em Execução',
    color: 'processing',
    icon: <SyncOutlined spin />,
  },
  atrasada: {
    label: 'Atrasada',
    color: 'red',
    icon: <WarningOutlined />,
  },
  perdida: {
    label: 'Perdida',
    color: 'default',
    icon: <CloseCircleOutlined />,
  },
};

const formatDate = (date?: Date | string | null) => {
  if (!date) return '-';
  const parsed = typeof date === 'string'
    ? new Date(`${date.substring(0, 10)}T00:00:00`)
    : date;
  return parsed.toLocaleDateString(getLocale());
};

const getRevisionEstimate = (revisao: RevisaoMotoResponse) => {
  const totalPecas = (revisao.pecas ?? []).reduce(
    (total, peca) => total + Number(peca.preco || 0) * Number(peca.quantidade || 0),
    0,
  );
  const totalServicos = (revisao.servicos ?? []).reduce(
    (total, servico) => total + Number(servico.custo || 0),
    0,
  );
  return totalPecas + totalServicos;
};

function AgendamentoModal({
  item,
  lojas,
  title,
  okText,
  open,
  submitting,
  onCancel,
  onConfirm,
}: AgendamentoModalProps) {
  const [form] = Form.useForm();

  useEffect(() => {
    if (!open || !item) return;

    const dataInicial = item.revisao.dataAgendamento
      ? dayjs(item.revisao.dataAgendamento.substring(0, 10))
      : dayjs(item.dataIdeal);
    form.setFieldsValue({
      lojaId: item.revisao.lojaId,
      dataAgendamento: dataInicial.isBefore(dayjs(), 'day') ? undefined : dataInicial,
    });
  }, [form, item, open]);

  if (!item) return null;

  const disabledDate = (date: Dayjs) => {
    const hoje = dayjs().startOf('day');
    const minima = dayjs(item.dataMinima).startOf('day');
    const limite = dayjs(item.dataLimite).startOf('day');
    return date.isBefore(hoje, 'day') || date.isBefore(minima, 'day') || date.isAfter(limite, 'day');
  };

  const handleOk = async () => {
    const values = await form.validateFields();
    await onConfirm(values.lojaId, values.dataAgendamento.format('YYYY-MM-DD'));
    form.resetFields();
  };

  return (
    <Modal
      title={title}
      open={open}
      okText={okText}
      cancelText="Cancelar"
      confirmLoading={submitting}
      onOk={handleOk}
      onCancel={() => {
        form.resetFields();
        onCancel();
      }}
      destroyOnClose
    >
      <Flex vertical gap="middle">
        <Alert
          type="info"
          showIcon
          message={`${item.moto.marca} ${item.moto.nomeModelo} · ${item.revisao.ordem}ª revisão`}
          description={`Janela permitida: ${formatDate(item.dataMinima)} até ${formatDate(item.dataLimite)}.`}
        />
        <Form form={form} layout="vertical">
          <Form.Item
            label="Concessionária"
            name="lojaId"
            rules={[{ required: true, message: 'Selecione uma concessionária' }]}
          >
            <Select
              showSearch
              optionFilterProp="label"
              placeholder="Selecione onde deseja fazer a revisão"
              options={lojas.map((loja) => ({
                value: loja.id,
                label: `${loja.nome} · ${loja.cidade}/${loja.uf}`,
              }))}
            />
          </Form.Item>
          <Form.Item
            label="Data da revisão"
            name="dataAgendamento"
            rules={[{ required: true, message: 'Selecione uma data' }]}
          >
            <DatePicker
              style={{ width: '100%' }}
              format="DD/MM/YYYY"
              disabledDate={disabledDate}
              placeholder="Selecione a data"
            />
          </Form.Item>
        </Form>
        <Text type="secondary">
          Após o envio, a revisão ficará como <Text strong>Aguardando Confirmação</Text> até a concessionária confirmar.
        </Text>
      </Flex>
    </Modal>
  );
}

function DetalhesDrawer({
  item,
  onClose,
}: {
  item: AgendamentoItem | null;
  onClose: () => void;
}) {
  if (!item) return null;

  const config = statusConfig[item.status];
  const revisao = item.revisao;

  return (
    <Drawer title="Detalhes da revisão" width={560} open={Boolean(item)} onClose={onClose}>
      <Flex vertical gap="large">
        <Descriptions bordered size="small" column={1}>
          <Descriptions.Item label="Status">
            <Tag color={config.color} icon={config.icon}>{config.label}</Tag>
          </Descriptions.Item>
          <Descriptions.Item label="Moto">
            {item.moto.marca} {item.moto.nomeModelo} · {item.moto.placa}
          </Descriptions.Item>
          <Descriptions.Item label="Revisão">
            {revisao.ordem}ª revisão · {formatIntegerInput(revisao.quilometragem)} km
          </Descriptions.Item>
          <Descriptions.Item label="Data ideal">{formatDate(item.dataIdeal)}</Descriptions.Item>
          <Descriptions.Item label="Janela">
            {formatDate(item.dataMinima)} até {formatDate(item.dataLimite)}
          </Descriptions.Item>
          <Descriptions.Item label="Data agendada">{formatDate(revisao.dataAgendamento)}</Descriptions.Item>
          <Descriptions.Item label="Concessionária">
            {revisao.nomeLoja ? `${revisao.nomeLoja} · ${revisao.cidadeLoja}/${revisao.ufLoja}` : '-'}
          </Descriptions.Item>
          <Descriptions.Item label="Estimativa">{formatCurrency(getRevisionEstimate(revisao))}</Descriptions.Item>
        </Descriptions>

        <Card title="Peças previstas" size="small">
          {revisao.pecas?.length ? (
            <Flex vertical gap={8}>
              {revisao.pecas.map((peca) => (
                <Flex key={peca.id} justify="space-between" gap="middle">
                  <Text>{peca.nome} x{peca.quantidade}</Text>
                  <Text strong>{formatCurrency(Number(peca.preco || 0) * Number(peca.quantidade || 0))}</Text>
                </Flex>
              ))}
            </Flex>
          ) : (
            <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Nenhuma peça prevista" />
          )}
        </Card>

        <Card title="Serviços previstos" size="small">
          {revisao.servicos?.length ? (
            <Flex vertical gap={8}>
              {revisao.servicos.map((servico) => (
                <Flex key={servico.id} justify="space-between" gap="middle">
                  <Text>{servico.nome}</Text>
                  <Text strong>{formatCurrency(servico.custo)}</Text>
                </Flex>
              ))}
            </Flex>
          ) : (
            <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Nenhum serviço previsto" />
          )}
        </Card>
      </Flex>
    </Drawer>
  );
}

function AgendamentoCard({
  item,
  lojas,
  onAgendar,
  onRemarcar,
  onCancelar,
  onDetalhes,
  submittingId,
}: {
  item: AgendamentoItem;
  lojas: Loja[];
  onAgendar: (item: AgendamentoItem, lojaId: number, dataAgendamento: string) => Promise<void>;
  onRemarcar: (item: AgendamentoItem, lojaId: number, dataAgendamento: string) => Promise<void>;
  onCancelar: (item: AgendamentoItem) => Promise<void>;
  onDetalhes: (item: AgendamentoItem) => void;
  submittingId: number | null;
}) {
  const [agendarOpen, setAgendarOpen] = useState(false);
  const [remarcarOpen, setRemarcarOpen] = useState(false);
  const config = statusConfig[item.status];
  const isDisponivel = item.status === AGENDAMENTO_STATUS.AGUARDANDO_AGENDAMENTO || item.status === AGENDAMENTO_STATUS.ATRASADA;
  const isAgendada = item.status === AGENDAMENTO_STATUS.AGENDADA;
  const isAguardandoConfirmacao = item.status === AGENDAMENTO_STATUS.AGUARDANDO_CONFIRMACAO;
  const isWorking = submittingId === item.revisao.id;

  return (
    <>
      <Card style={{ height: '100%' }} styles={{ body: { height: '100%' } }}>
        <Flex vertical gap="middle" style={{ height: '100%' }}>
          <Flex justify="space-between" gap="middle" align="flex-start">
            <Flex vertical gap={6}>
              <Tag color={config.color} icon={config.icon} style={{ width: 'fit-content' }}>
                {config.label}
              </Tag>
              <Title level={4} style={{ margin: 0 }}>{item.revisao.ordem}ª Revisão</Title>
            </Flex>
            <Button type="text" icon={<EyeOutlined />} onClick={() => onDetalhes(item)} />
          </Flex>

          <Flex vertical gap={8}>
            <Flex gap={8} align="center">
              <CarOutlined style={{ color: '#8c8c8c' }} />
              <Text>{item.moto.marca} {item.moto.nomeModelo} · {item.moto.placa}</Text>
            </Flex>
            <Flex gap={8} align="center">
              <CalendarOutlined style={{ color: '#8c8c8c' }} />
              <Text type="secondary">Data ideal:</Text>
              <Text>{formatDate(item.dataIdeal)}</Text>
            </Flex>
            {item.revisao.dataAgendamento && (
              <Flex gap={8} align="center">
                <CalendarOutlined style={{ color: '#1677ff' }} />
                <Text type="secondary">
                  {isAguardandoConfirmacao ? 'Data solicitada:' : 'Data agendada:'}
                </Text>
                <Text strong>{formatDate(item.revisao.dataAgendamento)}</Text>
              </Flex>
            )}
            {item.revisao.nomeLoja && (
              <Flex gap={8} align="center">
                <EnvironmentOutlined style={{ color: '#8c8c8c' }} />
                <Text>{item.revisao.nomeLoja} · {item.revisao.cidadeLoja}/{item.revisao.ufLoja}</Text>
              </Flex>
            )}
            <Flex gap={8} align="center">
              <ToolOutlined style={{ color: '#8c8c8c' }} />
              <Text type="secondary">
                {(item.revisao.pecas ?? []).length} peça(s) · {(item.revisao.servicos ?? []).length} serviço(s)
              </Text>
            </Flex>
          </Flex>

          {(item.status === AGENDAMENTO_STATUS.ATRASADA || item.status === AGENDAMENTO_STATUS.PERDIDA) && (
            <Alert
              type={item.status === AGENDAMENTO_STATUS.PERDIDA ? 'error' : 'warning'}
              showIcon
              message={item.status === AGENDAMENTO_STATUS.PERDIDA ? 'Prazo perdido' : 'Agendamento atrasado'}
              description={item.prazoTexto}
            />
          )}

          {isAguardandoConfirmacao && (
            <Alert
              type="warning"
              showIcon
              message="Solicitação enviada"
              description="A concessionária precisa confirmar antes da revisão ficar agendada."
            />
          )}

          <Text type={isDisponivel ? 'warning' : 'secondary'}>{item.prazoTexto}</Text>
          <div style={{ flex: 1 }} />

          <Flex justify="flex-end" gap={8} wrap>
            {(isDisponivel && item.status !== AGENDAMENTO_STATUS.PERDIDA) && (
              <Button type="primary" icon={<CalendarOutlined />} onClick={() => setAgendarOpen(true)}>
                {item.status === AGENDAMENTO_STATUS.ATRASADA ? 'Reagendar' : 'Agendar'}
              </Button>
            )}
            {isAgendada && (
              <Button type="primary" icon={<CalendarOutlined />} onClick={() => setRemarcarOpen(true)}>
                Remarcar
              </Button>
            )}
            {(isAgendada || isAguardandoConfirmacao) && (
              <Popconfirm
                title="Cancelar agendamento"
                description="A revisão voltará para Aguardando Agendamento."
                okText="Sim, cancelar"
                cancelText="Não"
                okButtonProps={{ danger: true, loading: isWorking }}
                onConfirm={() => onCancelar(item)}
              >
                <Button danger loading={isWorking}>Cancelar</Button>
              </Popconfirm>
            )}
          </Flex>
        </Flex>
      </Card>

      <AgendamentoModal
        item={item}
        lojas={lojas}
        title={item.status === AGENDAMENTO_STATUS.ATRASADA ? 'Reagendar Revisão' : 'Agendar Revisão'}
        okText="Enviar Solicitação"
        open={agendarOpen}
        submitting={isWorking}
        onCancel={() => setAgendarOpen(false)}
        onConfirm={async (lojaId, dataAgendamento) => {
          await onAgendar(item, lojaId, dataAgendamento);
          setAgendarOpen(false);
        }}
      />

      <AgendamentoModal
        item={item}
        lojas={lojas}
        title="Remarcar Revisão"
        okText="Enviar Remarcação"
        open={remarcarOpen}
        submitting={isWorking}
        onCancel={() => setRemarcarOpen(false)}
        onConfirm={async (lojaId, dataAgendamento) => {
          await onRemarcar(item, lojaId, dataAgendamento);
          setRemarcarOpen(false);
        }}
      />
    </>
  );
}

function Section({
  title,
  items,
  lojas,
  onAgendar,
  onRemarcar,
  onCancelar,
  onDetalhes,
  submittingId,
}: {
  title: string;
  items: AgendamentoItem[];
  lojas: Loja[];
  onAgendar: (item: AgendamentoItem, lojaId: number, dataAgendamento: string) => Promise<void>;
  onRemarcar: (item: AgendamentoItem, lojaId: number, dataAgendamento: string) => Promise<void>;
  onCancelar: (item: AgendamentoItem) => Promise<void>;
  onDetalhes: (item: AgendamentoItem) => void;
  submittingId: number | null;
}) {
  if (!items.length) return null;

  return (
    <Flex vertical gap="middle">
      <Title level={4} style={{ margin: 0 }}>{title}</Title>
      <Row gutter={[16, 16]}>
        {items.map((item) => (
          <Col xs={24} lg={12} xl={8} key={item.key}>
            <AgendamentoCard
              item={item}
              lojas={lojas}
              onAgendar={onAgendar}
              onRemarcar={onRemarcar}
              onCancelar={onCancelar}
              onDetalhes={onDetalhes}
              submittingId={submittingId}
            />
          </Col>
        ))}
      </Row>
    </Flex>
  );
}

export default function AgendamentosCliente() {
  const [motos, setMotos] = useState<Moto[]>([]);
  const [lojas, setLojas] = useState<Loja[]>([]);
  const [loading, setLoading] = useState(true);
  const [submittingId, setSubmittingId] = useState<number | null>(null);
  const [detalhes, setDetalhes] = useState<AgendamentoItem | null>(null);

  const loadData = async () => {
    try {
      setLoading(true);
      const [motosData, lojasData] = await Promise.all([
        motoService.getAll(),
        concessionariaService.getLojasAtivas(),
      ]);
      setMotos(motosData);
      setLojas(lojasData.filter((loja) => loja.ativo));
    } catch (error) {
      handleApiError(error, 'error.unexpected');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const itens = useMemo<AgendamentoItem[]>(
    () => buildAgendamentoItems(motos, new Date()) as AgendamentoItem[],
    [motos],
  );

  const grouped = {
    emExecucao: itens.filter((item) => item.status === AGENDAMENTO_STATUS.EM_EXECUCAO),
    aguardandoConfirmacao: itens.filter((item) => item.status === AGENDAMENTO_STATUS.AGUARDANDO_CONFIRMACAO),
    agendadas: itens.filter((item) => item.status === AGENDAMENTO_STATUS.AGENDADA),
    disponiveis: itens.filter((item) =>
      item.status === AGENDAMENTO_STATUS.AGUARDANDO_AGENDAMENTO ||
      item.status === AGENDAMENTO_STATUS.ATRASADA
    ),
    perdidas: itens.filter((item) => item.status === AGENDAMENTO_STATUS.PERDIDA),
  };

  const handleAgendar = async (item: AgendamentoItem, lojaId: number, dataAgendamento: string) => {
    try {
      setSubmittingId(item.revisao.id);
      await motoService.solicitarAgendamento(item.revisao.id, { lojaId, dataAgendamento });
      message.success('Solicitação de agendamento enviada.');
      await loadData();
    } catch (error) {
      handleApiError(error, 'error.unexpected');
    } finally {
      setSubmittingId(null);
    }
  };

  const handleRemarcar = async (item: AgendamentoItem, lojaId: number, dataAgendamento: string) => {
    try {
      setSubmittingId(item.revisao.id);
      await motoService.remarcarAgendamento(item.revisao.id, { lojaId, dataAgendamento });
      message.success('Solicitação de remarcação enviada.');
      await loadData();
    } catch (error) {
      handleApiError(error, 'error.unexpected');
    } finally {
      setSubmittingId(null);
    }
  };

  const handleCancelar = async (item: AgendamentoItem) => {
    try {
      setSubmittingId(item.revisao.id);
      await motoService.cancelarAgendamento(item.revisao.id);
      message.success('Agendamento cancelado.');
      await loadData();
    } catch (error) {
      handleApiError(error, 'error.unexpected');
    } finally {
      setSubmittingId(null);
    }
  };

  if (loading) {
    return (
      <Flex justify="center" align="center" style={{ minHeight: 360 }}>
        <Spin size="large" />
      </Flex>
    );
  }

  return (
    <Flex vertical gap="large">
      <DashboardBreadcrumb
        userType="cliente"
        items={[{ title: 'Agendamentos', icon: <CalendarOutlined /> }]}
      />

      <Flex justify="space-between" align="flex-start" gap="middle" wrap>
        <Flex vertical gap={4}>
          <Title level={2} style={{ margin: 0 }}>Agendamentos</Title>
          <Text type="secondary">
            Acompanhe revisões dentro da janela de D-15 a D+15, solicitações pendentes e agendamentos confirmados.
          </Text>
        </Flex>
        <Button onClick={loadData}>Atualizar</Button>
      </Flex>

      <Row gutter={[16, 16]}>
        <Col xs={12} md={8} xl={4}>
          <Card><Statistic title="Em execução" value={grouped.emExecucao.length} prefix={<SyncOutlined />} /></Card>
        </Col>
        <Col xs={12} md={8} xl={5}>
          <Card><Statistic title="Aguard. confirmação" value={grouped.aguardandoConfirmacao.length} prefix={<HourglassOutlined />} /></Card>
        </Col>
        <Col xs={12} md={8} xl={4}>
          <Card><Statistic title="Agendadas" value={grouped.agendadas.length} prefix={<CalendarOutlined />} /></Card>
        </Col>
        <Col xs={12} md={8} xl={5}>
          <Card><Statistic title="Disponíveis" value={grouped.disponiveis.length} prefix={<ClockCircleOutlined />} /></Card>
        </Col>
        <Col xs={12} md={8} xl={4}>
          <Card><Statistic title="Perdidas" value={grouped.perdidas.length} prefix={<CloseCircleOutlined />} /></Card>
        </Col>
      </Row>

      {!itens.length ? (
        <Card>
          <Empty description="Nenhuma revisão disponível para agendamento no momento." />
        </Card>
      ) : (
        <Flex vertical gap="large">
          <Section title="Em execução" items={grouped.emExecucao} lojas={lojas} onAgendar={handleAgendar} onRemarcar={handleRemarcar} onCancelar={handleCancelar} onDetalhes={setDetalhes} submittingId={submittingId} />
          <Section title="Aguardando confirmação" items={grouped.aguardandoConfirmacao} lojas={lojas} onAgendar={handleAgendar} onRemarcar={handleRemarcar} onCancelar={handleCancelar} onDetalhes={setDetalhes} submittingId={submittingId} />
          <Section title="Agendamentos confirmados" items={grouped.agendadas} lojas={lojas} onAgendar={handleAgendar} onRemarcar={handleRemarcar} onCancelar={handleCancelar} onDetalhes={setDetalhes} submittingId={submittingId} />
          <Section title="Disponíveis para agendar" items={grouped.disponiveis} lojas={lojas} onAgendar={handleAgendar} onRemarcar={handleRemarcar} onCancelar={handleCancelar} onDetalhes={setDetalhes} submittingId={submittingId} />
          <Section title="Perdidas" items={grouped.perdidas} lojas={lojas} onAgendar={handleAgendar} onRemarcar={handleRemarcar} onCancelar={handleCancelar} onDetalhes={setDetalhes} submittingId={submittingId} />
        </Flex>
      )}

      <DetalhesDrawer item={detalhes} onClose={() => setDetalhes(null)} />
    </Flex>
  );
}
