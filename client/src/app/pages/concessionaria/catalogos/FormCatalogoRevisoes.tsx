import { useState, useMemo, useEffect } from 'react';
import {
  Breadcrumb,
  Typography,
  Steps,
  Button,
  Form,
  Input,
  Select,
  InputNumber,
  Table,
  Tabs,
  Alert,
  Flex,
  Modal,
  Descriptions,
  Tag,
  Card,
  App,
} from 'antd';
import {
  HomeOutlined,
  ToolOutlined,
  ArrowLeftOutlined,
  DeleteOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useLinhas } from '../../contexts/LinhasContext';
import { useMotos } from '../../contexts/MotosContext';
import { useModelosRevisao, type RevisaoPecaItem, type RevisaoServicoItem, type RevisaoConfig } from '../../contexts/ModelosRevisaoContext';

const { Title, Text } = Typography;

interface CatalogoModelosRevisaoCreateProps {
  onBack: () => void;
  editModeloKey?: string;
}

const CATALOGO_PECAS = [
  { key: 'P001', codigo: 'P001', nome: 'Filtro de Óleo', preco: 35 },
  { key: 'P002', codigo: 'P002', nome: 'Vela de Ignição NGK', preco: 18 },
  { key: 'P003', codigo: 'P003', nome: 'Pastilha de Freio', preco: 85 },
  { key: 'P004', codigo: 'P004', nome: 'Correia Dentada', preco: 120 },
];

const CATALOGO_SERVICOS = [
  { key: 'S001', nome: 'Troca de Óleo', tempoMinutos: 30, valor: 80 },
  { key: 'S002', nome: 'Verificação de Pneus', tempoMinutos: 15, valor: 50 },
  { key: 'S003', nome: 'Lubrificação de Cabos', tempoMinutos: 20, valor: 60 },
  { key: 'S004', nome: 'Ajuste de Freios', tempoMinutos: 45, valor: 120 },
];

const DEFAULT_REVISAO_VALUES = [
  { numero: 1, quilometragem: 1000, tempoMeses: 6 },
  { numero: 2, quilometragem: 5000, tempoMeses: 12 },
  { numero: 3, quilometragem: 10000, tempoMeses: 18 },
  { numero: 4, quilometragem: 15000, tempoMeses: 24 },
  { numero: 5, quilometragem: 20000, tempoMeses: 30 },
  { numero: 6, quilometragem: 25000, tempoMeses: 36 },
  { numero: 7, quilometragem: 30000, tempoMeses: 42 },
];

const formatCurrency = (v: number) =>
  v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

const formatTempo = (minutos: number) => {
  if (minutos === 0) return '-';
  const h = Math.floor(minutos / 60);
  const m = minutos % 60;
  if (h === 0) return `${m}min`;
  return m > 0 ? `${h}h ${m}min` : `${h}h`;
};

const calcRevisao = (rev: RevisaoConfig) => {
  const totalPecasValor = rev.pecas.reduce((s, p) => s + p.valorTotal, 0);
  const totalServicosValor = rev.servicos.reduce((s, sv) => s + sv.valorMaoObra, 0);
  const tempoMinutos = rev.servicos.reduce((s, sv) => s + sv.tempoMinutos, 0);
  return {
    totalPecasValor,
    totalServicosValor,
    tempoMinutos,
    valorTotal: totalPecasValor + totalServicosValor,
  };
};

const ordinal = (n: number) => `${n}ª`;

export default function CatalogoModelosRevisaoCreate({ onBack, editModeloKey }: CatalogoModelosRevisaoCreateProps) {
  const { linhas } = useLinhas();
  const { motos } = useMotos();
  const { modelosRevisao, setModelosRevisao } = useModelosRevisao();
  const { modal } = App.useApp();

  const [currentStep, setCurrentStep] = useState(0);
  const [step1Form] = Form.useForm();
  const [step1Data, setStep1Data] = useState<{ nome: string; linha: string; quantidadeRevisoes: 4 | 7 } | null>(null);
  const [linhaSelected, setLinhaSelected] = useState('');
  const [revisoes, setRevisoes] = useState<RevisaoConfig[]>([]);
  const [activeRevTab, setActiveRevTab] = useState('0');
  const [addPecaKey, setAddPecaKey] = useState<string | null>(null);
  const [addPecaQty, setAddPecaQty] = useState(1);
  const [addServKey, setAddServKey] = useState<string | null>(null);

  // Pre-populate when editing an existing modelo (draft)
  useEffect(() => {
    if (!editModeloKey) return;
    const modelo = modelosRevisao.find((m) => m.key === editModeloKey);
    if (!modelo) return;
    const qty = modelo.quantidadeRevisoes as 4 | 7;
    step1Form.setFieldsValue({ nome: modelo.nome, linha: modelo.linha, quantidadeRevisoes: qty });
    setLinhaSelected(modelo.linha);
    setStep1Data({ nome: modelo.nome, linha: modelo.linha, quantidadeRevisoes: qty });
    setRevisoes(
      DEFAULT_REVISAO_VALUES.slice(0, qty).map((d) => ({ ...d, pecas: [], servicos: [] })),
    );
  }, [editModeloKey]);

  const motosNaLinha = useMemo(
    () => motos.filter((m) => m.linha === linhaSelected),
    [motos, linhaSelected],
  );

  const linhaJaTemAtivo = useMemo(
    () => modelosRevisao.some((m) => m.linha === linhaSelected && m.status === 'ativo'),
    [modelosRevisao, linhaSelected],
  );

  const handleQtdChange = (val: number) => {
    setRevisoes(
      DEFAULT_REVISAO_VALUES.slice(0, val).map((d) => ({
        ...d,
        pecas: [],
        servicos: [],
      })),
    );
    setActiveRevTab('0');
  };

  const updateRevisaoField = (
    idx: number,
    field: 'quilometragem' | 'tempoMeses',
    value: number | null,
  ) => {
    setRevisoes((prev) =>
      prev.map((r, i) => (i === idx ? { ...r, [field]: value ?? 0 } : r)),
    );
  };

  const addPeca = (revIdx: number) => {
    if (!addPecaKey) return;
    const catalogo = CATALOGO_PECAS.find((p) => p.key === addPecaKey);
    if (!catalogo) return;
    const qty = addPecaQty > 0 ? addPecaQty : 1;
    const newItem: RevisaoPecaItem = {
      id: `${addPecaKey}-${Date.now()}`,
      pecaKey: catalogo.key,
      nome: catalogo.nome,
      codigo: catalogo.codigo,
      quantidade: qty,
      valorUnitario: catalogo.preco,
      valorTotal: catalogo.preco * qty,
    };
    setRevisoes((prev) =>
      prev.map((r, i) =>
        i === revIdx ? { ...r, pecas: [...r.pecas, newItem] } : r,
      ),
    );
    setAddPecaKey(null);
    setAddPecaQty(1);
  };

  const removePeca = (revIdx: number, id: string) => {
    setRevisoes((prev) =>
      prev.map((r, i) =>
        i === revIdx ? { ...r, pecas: r.pecas.filter((p) => p.id !== id) } : r,
      ),
    );
  };

  const addServico = (revIdx: number) => {
    if (!addServKey) return;
    const catalogo = CATALOGO_SERVICOS.find((s) => s.key === addServKey);
    if (!catalogo) return;
    const newItem: RevisaoServicoItem = {
      id: `${addServKey}-${Date.now()}`,
      servicoKey: catalogo.key,
      nome: catalogo.nome,
      tempoMinutos: catalogo.tempoMinutos,
      valorMaoObra: catalogo.valor,
    };
    setRevisoes((prev) =>
      prev.map((r, i) =>
        i === revIdx ? { ...r, servicos: [...r.servicos, newItem] } : r,
      ),
    );
    setAddServKey(null);
  };

  const removeServico = (revIdx: number, id: string) => {
    setRevisoes((prev) =>
      prev.map((r, i) =>
        i === revIdx ? { ...r, servicos: r.servicos.filter((s) => s.id !== id) } : r,
      ),
    );
  };

  const goNext = async () => {
    if (currentStep === 0) {
      try {
        const vals = await step1Form.validateFields();
        setStep1Data(vals as { nome: string; linha: string; quantidadeRevisoes: 4 | 7 });
      } catch {
        return;
      }
    }
    setCurrentStep((s) => s + 1);
  };

  const handleSave = (status: 'ativo' | 'rascunho') => {
    const values = step1Data ?? (step1Form.getFieldsValue() as { nome: string; linha: string; quantidadeRevisoes: 4 | 7 });

    if (status === 'ativo' && linhaJaTemAtivo) {
      Modal.error({
        title: 'Não é possível publicar',
        content:
          'Esta linha já possui um Modelo de Revisão ativo. Desative o modelo existente antes de publicar um novo.',
      });
      return;
    }

    const totalValor = revisoes.reduce((s, r) => s + calcRevisao(r).valorTotal, 0);
    const totalTempo = revisoes.reduce((s, r) => s + calcRevisao(r).tempoMinutos, 0);

    const novoModelo = {
      key: editModeloKey ?? Date.now().toString(),
      nome: values.nome,
      linha: values.linha,
      quantidadeRevisoes: values.quantidadeRevisoes,
      modelosVinculados: motosNaLinha.map((m) => `${m.marca} ${m.modelo}`),
      valorMedioTotal: revisoes.length > 0 ? formatCurrency(totalValor / revisoes.length) : '-',
      tempoEstimadoTotal: formatTempo(totalTempo),
      status,
      revisoes,
    };

    const persist = () => {
      if (editModeloKey) {
        // Replace existing draft
        setModelosRevisao((prev) => prev.map((m) => (m.key === editModeloKey ? novoModelo : m)));
      } else {
        setModelosRevisao((prev) => [...prev, novoModelo]);
      }
      onBack();
    };

    if (status === 'ativo') {
      modal.confirm({
        title: 'Publicar Modelo de Revisão',
        content: `Este Modelo de Revisão será aplicado automaticamente aos Modelos de Moto vinculados à Linha "${values.linha}".`,
        okText: 'Publicar',
        cancelText: 'Cancelar',
        onOk: persist,
      });
    } else {
      persist();
    }
  };

  // ─── Step 1 ───────────────────────────────────────────────────────────────
  const renderStep1 = () => (
    <Card title="Dados Gerais">
      <Form form={step1Form} layout="vertical" style={{ maxWidth: 600 }}>
        <Form.Item
          name="nome"
          label="Nome do Modelo"
          rules={[{ required: true, message: 'Informe o nome do modelo' }]}
        >
          <Input placeholder="Ex: Modelo Padrão - Passeio" />
        </Form.Item>

        <Form.Item
          name="linha"
          label="Linha de Moto"
          rules={[{ required: true, message: 'Selecione a linha' }]}
        >
          <Select
            placeholder="Selecione uma linha"
            style={{ width: '100%' }}
            onChange={(val: string) => {
              setLinhaSelected(val);
              const qty = step1Form.getFieldValue('quantidadeRevisoes');
              if (qty) handleQtdChange(qty);
            }}
            options={linhas.map((l) => ({ value: l.nome, label: l.nome }))}
          />
        </Form.Item>

        {linhaSelected && (
          <Form.Item>
            {linhaJaTemAtivo ? (
              <Alert
                type="warning"
                showIcon
                message="Esta linha já possui um Modelo de Revisão ativo"
                description="Você só poderá salvar este modelo como Rascunho. Para publicá-lo, desative o modelo existente."
              />
            ) : (
              <Alert
                type="info"
                showIcon
                message={`Linha ${linhaSelected}: ${motosNaLinha.length} modelo(s) de moto vinculado(s)`}
                description={
                  <Flex vertical gap="small">
                    {motosNaLinha.slice(0, 5).map((m) => (
                      <Text key={m.key}>
                        {m.marca} {m.modelo} — {m.ano}
                      </Text>
                    ))}
                    <Text>
                      Os Modelos de Moto vinculados à Linha {linhaSelected} usarão este Modelo de
                      Revisão automaticamente.
                    </Text>
                  </Flex>
                }
              />
            )}
          </Form.Item>
        )}

        <Form.Item
          name="quantidadeRevisoes"
          label="Quantidade de Revisões"
          rules={[{ required: true, message: 'Selecione a quantidade de revisões' }]}
        >
          <Select
            placeholder="Selecione"
            style={{ width: '100%' }}
            onChange={(val: number) => handleQtdChange(val)}
            options={[
              { value: 4, label: '4 revisões' },
              { value: 7, label: '7 revisões' },
            ]}
          />
        </Form.Item>

        <Form.Item label="Status">
          <Flex gap="small" align="center">
            <Tag color="blue">Rascunho</Tag>
            <Text type="secondary">
              O modelo será salvo como rascunho e poderá ser publicado posteriormente.
            </Text>
          </Flex>
        </Form.Item>
      </Form>
    </Card>
  );

  // ─── Step 2 ───────────────────────────────────────────────────────────────
  const step2Columns: ColumnsType<RevisaoConfig> = [
    {
      title: 'Nº Revisão',
      dataIndex: 'numero',
      key: 'numero',
      width: 120,
      render: (n: number) => ordinal(n),
    },
    {
      title: 'Quilometragem Máxima',
      dataIndex: 'quilometragem',
      key: 'quilometragem',
      render: (_: number, _record: RevisaoConfig, idx: number) => (
        <InputNumber
          style={{ width: '100%' }}
          min={0}
          addonAfter="km"
          value={revisoes[idx]?.quilometragem}
          onChange={(val) => updateRevisaoField(idx, 'quilometragem', val)}
        />
      ),
    },
    {
      title: 'Tempo Máximo',
      dataIndex: 'tempoMeses',
      key: 'tempoMeses',
      render: (_: number, _record: RevisaoConfig, idx: number) => (
        <InputNumber
          style={{ width: '100%' }}
          min={0}
          addonAfter="meses"
          value={revisoes[idx]?.tempoMeses}
          onChange={(val) => updateRevisaoField(idx, 'tempoMeses', val)}
        />
      ),
    },
  ];

  const renderStep2 = () => (
    <Card title="Estrutura das Revisões">
      <Table
        dataSource={revisoes}
        columns={step2Columns}
        rowKey="numero"
        pagination={false}
        bordered
      />
    </Card>
  );

  // ─── Step 3 ───────────────────────────────────────────────────────────────
  const renderRevisaoTab = (rev: RevisaoConfig, revIdx: number) => {
    const { totalPecasValor, totalServicosValor, tempoMinutos, valorTotal } = calcRevisao(rev);

    const pecasJaAdicionadas = new Set(rev.pecas.map((p) => p.pecaKey));
    const servicosJaAdicionados = new Set(rev.servicos.map((s) => s.servicoKey));

    const pecasDisponiveis = CATALOGO_PECAS.filter((p) => !pecasJaAdicionadas.has(p.key));
    const servicosDisponiveis = CATALOGO_SERVICOS.filter(
      (s) => !servicosJaAdicionados.has(s.key),
    );

    const pecasColumns: ColumnsType<RevisaoPecaItem> = [
      { title: 'Código', dataIndex: 'codigo', key: 'codigo', width: 100 },
      { title: 'Peça', dataIndex: 'nome', key: 'nome' },
      { title: 'Qtd', dataIndex: 'quantidade', key: 'quantidade', width: 80, align: 'center' },
      {
        title: 'Valor Unitário',
        dataIndex: 'valorUnitario',
        key: 'valorUnitario',
        width: 130,
        render: (v: number) => formatCurrency(v),
      },
      {
        title: 'Valor Total',
        dataIndex: 'valorTotal',
        key: 'valorTotal',
        width: 130,
        render: (v: number) => formatCurrency(v),
      },
      {
        title: '',
        key: 'actions',
        width: 60,
        render: (_: any, record: RevisaoPecaItem) => (
          <Button
            type="link"
            danger
            icon={<DeleteOutlined />}
            onClick={() => removePeca(revIdx, record.id)}
          />
        ),
      },
    ];

    const servicosColumns: ColumnsType<RevisaoServicoItem> = [
      { title: 'Serviço', dataIndex: 'nome', key: 'nome' },
      {
        title: 'Tempo Médio',
        dataIndex: 'tempoMinutos',
        key: 'tempoMinutos',
        width: 130,
        render: (v: number) => formatTempo(v),
      },
      {
        title: 'Valor Mão de Obra',
        dataIndex: 'valorMaoObra',
        key: 'valorMaoObra',
        width: 160,
        render: (v: number) => formatCurrency(v),
      },
      {
        title: '',
        key: 'actions',
        width: 60,
        render: (_: any, record: RevisaoServicoItem) => (
          <Button
            type="link"
            danger
            icon={<DeleteOutlined />}
            onClick={() => removeServico(revIdx, record.id)}
          />
        ),
      },
    ];

    return (
      <Flex vertical gap="middle">
        <Descriptions size="small" bordered>
          <Descriptions.Item label="KM Máximo">
            {rev.quilometragem.toLocaleString('pt-BR')} km
          </Descriptions.Item>
          <Descriptions.Item label="Tempo Máximo">{rev.tempoMeses} meses</Descriptions.Item>
          <Descriptions.Item label="Valor Estimado">{formatCurrency(valorTotal)}</Descriptions.Item>
        </Descriptions>

        <Tabs
          size="small"
          items={[
            {
              key: 'pecas',
              label: `Peças (${rev.pecas.length})`,
              children: (
                <Flex vertical gap="middle">
                  <Flex gap="middle" align="center">
                    <Select
                      style={{ flex: 1 }}
                      placeholder="Selecione uma peça"
                      value={addPecaKey}
                      onChange={(val) => setAddPecaKey(val)}
                      options={pecasDisponiveis.map((p) => ({
                        value: p.key,
                        label: `${p.codigo} — ${p.nome} (${formatCurrency(p.preco)})`,
                      }))}
                    />
                    <InputNumber
                      min={1}
                      value={addPecaQty}
                      onChange={(val) => setAddPecaQty(val ?? 1)}
                      style={{ width: 80 }}
                    />
                    <Button
                      type="primary"
                      disabled={!addPecaKey}
                      onClick={() => addPeca(revIdx)}
                    >
                      Adicionar
                    </Button>
                  </Flex>
                  <Table
                    dataSource={rev.pecas}
                    columns={pecasColumns}
                    rowKey="id"
                    pagination={false}
                    bordered
                    size="small"
                    summary={() => (
                      <Table.Summary.Row>
                        <Table.Summary.Cell index={0} colSpan={4} align="right">
                          <Text strong>Total Peças:</Text>
                        </Table.Summary.Cell>
                        <Table.Summary.Cell index={1}>
                          <Text strong>{formatCurrency(totalPecasValor)}</Text>
                        </Table.Summary.Cell>
                        <Table.Summary.Cell index={2} />
                      </Table.Summary.Row>
                    )}
                  />
                </Flex>
              ),
            },
            {
              key: 'servicos',
              label: `Serviços (${rev.servicos.length})`,
              children: (
                <Flex vertical gap="middle">
                  <Flex gap="middle" align="center">
                    <Select
                      style={{ flex: 1 }}
                      placeholder="Selecione um serviço"
                      value={addServKey}
                      onChange={(val) => setAddServKey(val)}
                      options={servicosDisponiveis.map((s) => ({
                        value: s.key,
                        label: `${s.nome} — ${s.tempoMinutos}min — ${formatCurrency(s.valor)}`,
                      }))}
                    />
                    <Button
                      type="primary"
                      disabled={!addServKey}
                      onClick={() => addServico(revIdx)}
                    >
                      Adicionar
                    </Button>
                  </Flex>
                  <Table
                    dataSource={rev.servicos}
                    columns={servicosColumns}
                    rowKey="id"
                    pagination={false}
                    bordered
                    size="small"
                    summary={() => (
                      <Table.Summary.Row>
                        <Table.Summary.Cell index={0} align="right">
                          <Text strong>Total:</Text>
                        </Table.Summary.Cell>
                        <Table.Summary.Cell index={1}>
                          <Text strong>{formatTempo(tempoMinutos)}</Text>
                        </Table.Summary.Cell>
                        <Table.Summary.Cell index={2}>
                          <Text strong>{formatCurrency(totalServicosValor)}</Text>
                        </Table.Summary.Cell>
                        <Table.Summary.Cell index={3} />
                      </Table.Summary.Row>
                    )}
                  />
                </Flex>
              ),
            },
          ]}
        />
      </Flex>
    );
  };

  const renderStep3 = () => (
    <Card title="Peças e Serviços por Revisão">
      <Tabs
        activeKey={activeRevTab}
        onChange={(key) => {
          setActiveRevTab(key);
          setAddPecaKey(null);
          setAddPecaQty(1);
          setAddServKey(null);
        }}
        items={revisoes.map((rev, idx) => ({
          key: String(idx),
          label: `${ordinal(rev.numero)} Revisão`,
          children: renderRevisaoTab(rev, idx),
        }))}
      />
    </Card>
  );

  // ─── Step 4 ───────────────────────────────────────────────────────────────
  const renderStep4 = () => {
    const values = step1Data ?? (step1Form.getFieldsValue() as { nome: string; linha: string; quantidadeRevisoes: 4 | 7 });
    const totalPecasUnicas = new Set(revisoes.flatMap((r) => r.pecas.map((p) => p.pecaKey))).size;
    const totalServicosUnicos = new Set(
      revisoes.flatMap((r) => r.servicos.map((s) => s.servicoKey)),
    ).size;
    const totalTempo = revisoes.reduce((s, r) => s + calcRevisao(r).tempoMinutos, 0);
    const totalValor = revisoes.reduce((s, r) => s + calcRevisao(r).valorTotal, 0);

    const resumoColumns: ColumnsType<RevisaoConfig> = [
      {
        title: 'Nº Revisão',
        dataIndex: 'numero',
        key: 'numero',
        width: 110,
        render: (n: number) => ordinal(n),
      },
      {
        title: 'KM Máximo',
        dataIndex: 'quilometragem',
        key: 'quilometragem',
        render: (v: number) => `${v.toLocaleString('pt-BR')} km`,
      },
      {
        title: 'Tempo Máximo',
        dataIndex: 'tempoMeses',
        key: 'tempoMeses',
        render: (v: number) => `${v} meses`,
      },
      {
        title: 'Peças',
        key: 'pecas',
        width: 80,
        align: 'center',
        render: (_: any, record: RevisaoConfig) => record.pecas.length,
      },
      {
        title: 'Serviços',
        key: 'servicos',
        width: 90,
        align: 'center',
        render: (_: any, record: RevisaoConfig) => record.servicos.length,
      },
      {
        title: 'Tempo Estimado',
        key: 'tempo',
        width: 130,
        render: (_: any, record: RevisaoConfig) =>
          formatTempo(calcRevisao(record).tempoMinutos),
      },
      {
        title: 'Valor Médio',
        key: 'valor',
        width: 130,
        render: (_: any, record: RevisaoConfig) =>
          formatCurrency(calcRevisao(record).valorTotal),
      },
    ];

    return (
      <Flex vertical gap="large">
        {linhaJaTemAtivo && (
          <Alert
            type="warning"
            showIcon
            message="Publicação bloqueada"
            description="Esta linha já possui um Modelo de Revisão ativo. Você só pode salvar como Rascunho."
          />
        )}

        <Card title="Resumo do Modelo">
          <Descriptions bordered column={2}>
            <Descriptions.Item label="Nome do Modelo">{values.nome || '-'}</Descriptions.Item>
            <Descriptions.Item label="Linha de Moto">{values.linha || '-'}</Descriptions.Item>
            <Descriptions.Item label="Quantidade de Revisões">
              {values.quantidadeRevisoes || '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Modelos de Moto Herdeiros">
              {motosNaLinha.length} modelo(s)
            </Descriptions.Item>
            <Descriptions.Item label="Total de Peças Únicas">{totalPecasUnicas}</Descriptions.Item>
            <Descriptions.Item label="Total de Serviços Únicos">
              {totalServicosUnicos}
            </Descriptions.Item>
            <Descriptions.Item label="Tempo Estimado Total">
              {formatTempo(totalTempo)}
            </Descriptions.Item>
            <Descriptions.Item label="Valor Médio Total">
              {revisoes.length > 0 ? formatCurrency(totalValor / revisoes.length) : '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Status">
              <Tag color="blue">Rascunho</Tag>
            </Descriptions.Item>
          </Descriptions>
        </Card>

        <Card title="Tabela de Revisões">
          <Table
            dataSource={revisoes}
            columns={resumoColumns}
            rowKey="numero"
            pagination={false}
            bordered
            size="small"
          />
        </Card>
      </Flex>
    );
  };

  const stepItems = [
    { title: 'Dados Gerais' },
    { title: 'Estrutura das Revisões' },
    { title: 'Peças e Serviços' },
    { title: 'Revisão e Publicação' },
  ];

  const renderCurrentStep = () => {
    switch (currentStep) {
      case 0:
        return renderStep1();
      case 1:
        return renderStep2();
      case 2:
        return renderStep3();
      case 3:
        return renderStep4();
      default:
        return null;
    }
  };

  return (
    <Flex vertical gap="large" style={{ width: '100%' }}>
      {/* Header */}
      <Flex vertical gap="middle">
        <Breadcrumb
          items={[
            { href: '', title: <HomeOutlined /> },
            {
              title: (
                <>
                  <ToolOutlined />
                  <span> Catálogos</span>
                </>
              ),
            },
            { title: 'Modelos de Revisão' },
            { title: editModeloKey ? 'Editar Modelo de Revisão' : 'Novo Modelo de Revisão' },
          ]}
        />
        <Flex gap="middle" align="center">
          <Button icon={<ArrowLeftOutlined />} onClick={onBack} />
          <Title level={2} style={{ margin: 0 }}>
            {editModeloKey ? 'Editar Modelo de Revisão' : 'Novo Modelo de Revisão'}
          </Title>
        </Flex>
      </Flex>

      {/* Steps indicator */}
      <Steps current={currentStep} items={stepItems} />

      {/* Step content */}
      {renderCurrentStep()}

      {/* Navigation footer */}
      <Card>
        <Flex justify="space-between" align="center">
          <Button onClick={onBack}>Cancelar</Button>
          <Flex gap="middle" align="center">
            {currentStep > 0 && (
              <Button onClick={() => setCurrentStep((s) => s - 1)}>← Voltar</Button>
            )}
            {currentStep < 3 && (
              <Button type="primary" onClick={goNext}>
                Próximo →
              </Button>
            )}
            {currentStep === 3 && (
              <>
                <Button onClick={() => handleSave('rascunho')}>Salvar como Rascunho</Button>
                <Button
                  type="primary"
                  disabled={linhaJaTemAtivo}
                  onClick={() => handleSave('ativo')}
                >
                  Publicar Modelo de Revisão
                </Button>
              </>
            )}
          </Flex>
        </Flex>
      </Card>
    </Flex>
  );
}
