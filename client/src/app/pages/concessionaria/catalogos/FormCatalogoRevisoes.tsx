import { useEffect, useMemo, useState } from 'react';
import { useParams } from 'react-router';
import {
  Alert,
  Button,
  Card,
  Descriptions,
  Flex,
  Form,
  Input,
  InputNumber,
  Select,
  Spin,
  Steps,
  Table,
  Tabs,
  Tag,
  Typography,
  message,
  Modal,
  Breadcrumb,
} from 'antd';
import {
  ArrowLeftOutlined,
  DeleteOutlined,
  HomeOutlined,
  ToolOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { linhaService } from '@/app/services/linhaService';
import { modeloMotoService } from '@/app/services/modeloMotoService';
import { servicoService } from '@/app/services/servicoService';
import { pecaService, PecaResponse } from '@/app/services/pecaService';
import { revisaoPadraoService } from '@/app/services/revisaoPadraoService';
import { handleApiError } from '@/app/utils/errorHandler';
import { Linha } from '@/app/models/Linha';
import { ModeloMoto } from '@/app/models/ModeloMoto';
import { Servico } from '@/app/models/Servico';
import { RevisaoPadraoLinhaRequest, RevisaoPadraoPecaRequest } from '@/app/models/RevisaoPadraoRequest';
import { RevisaoPadraoListResponse } from '@/app/models/RevisaoPadrao';

const { Title, Text } = Typography;

interface CatalogoModelosRevisaoCreateProps {
  onBack: () => void;
}

interface RevisaoFormItem {
  key: string;
  ordem: number;
  nome: string;
  quilometragem: number;
  tempoMeses: number;
  servicosIds: number[];
  pecas: RevisaoPadraoPecaRequest[];
}

const DEFAULT_REVISAO_VALUES = [
  { ordem: 1, nome: 'Primeira revisão', quilometragem: 1000, tempoMeses: 6 },
  { ordem: 2, nome: 'Segunda revisão', quilometragem: 5000, tempoMeses: 12 },
  { ordem: 3, nome: 'Terceira revisão', quilometragem: 10000, tempoMeses: 18 },
  { ordem: 4, nome: 'Quarta revisão', quilometragem: 15000, tempoMeses: 24 },
  { ordem: 5, nome: 'Quinta revisão', quilometragem: 20000, tempoMeses: 30 },
  { ordem: 6, nome: 'Sexta revisão', quilometragem: 25000, tempoMeses: 36 },
  { ordem: 7, nome: 'Sétima revisão', quilometragem: 30000, tempoMeses: 42 },
];

const formatCurrency = (value: number) =>
  value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

const formatTempo = (minutos: number) => {
  if (minutos === 0) return '-';
  const h = Math.floor(minutos / 60);
  const m = minutos % 60;
  if (h === 0) return `${m}min`;
  return m > 0 ? `${h}h ${m}min` : `${h}h`;
};

const ordinal = (n: number) => `${n}ª`;

const createRevisoes = (quantity: number): RevisaoFormItem[] =>
  DEFAULT_REVISAO_VALUES.slice(0, quantity).map((item) => ({
    ...item,
    key: item.ordem.toString(),
    servicosIds: [],
    pecas: [],
  }));

export default function CatalogoModelosRevisaoCreate({ onBack }: CatalogoModelosRevisaoCreateProps) {
  const { linhaId: paramLinhaId } = useParams<{ linhaId?: string }>();
  const isEditMode = !!paramLinhaId;

  const [form] = Form.useForm();
  const [currentStep, setCurrentStep] = useState(0);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [linhas, setLinhas] = useState<Linha[]>([]);
  const [modelos, setModelos] = useState<ModeloMoto[]>([]);
  const [servicos, setServicos] = useState<Servico[]>([]);
  const [pecas, setPecas] = useState<PecaResponse[]>([]);
  const [revisoesExistentes, setRevisoesExistentes] = useState<RevisaoPadraoListResponse[]>([]);
  const [linhaId, setLinhaId] = useState<number | undefined>();
  const [revisoes, setRevisoes] = useState<RevisaoFormItem[]>(createRevisoes(4));
  const [activeRevTab, setActiveRevTab] = useState('0');
  const [nomeModelo, setNomeModelo] = useState('');

  // Input states for adding parts/services to revision
  const [addPecaId, setAddPecaId] = useState<number | null>(null);
  const [addPecaQty, setAddPecaQty] = useState(1);
  const [addServId, setAddServId] = useState<number | null>(null);

  useEffect(() => {
    const fetchDependencies = async () => {
      try {
        setLoading(true);
        const [linhasData, modelosData, servicosData, pecasData, revisoesExistentesData] = await Promise.all([
          linhaService.getAll(true),
          modeloMotoService.getAll(),
          servicoService.getAll(),
          pecaService.listar('Ativo'),
          revisaoPadraoService.listar(),
        ]);

        setLinhas(linhasData);
        setModelos(modelosData);
        setServicos(servicosData);
        setPecas(pecasData);
        setRevisoesExistentes(revisoesExistentesData);

        if (isEditMode && paramLinhaId) {
          const lid = Number(paramLinhaId);
          const revisoesDaLinha = revisoesExistentesData.filter((r) => r.linhaId === lid);
          
          if (revisoesDaLinha.length > 0) {
            const detailedRevisoes = await Promise.all(
              revisoesDaLinha.map((r) => revisaoPadraoService.getById(r.id))
            );

            detailedRevisoes.sort((a, b) => a.ordem - b.ordem);

            const mappedRevisoes: RevisaoFormItem[] = detailedRevisoes.map((rev) => ({
              key: rev.ordem.toString(),
              ordem: rev.ordem,
              nome: rev.nome,
              quilometragem: rev.quilometragem,
              tempoMeses: rev.tempoMeses,
              servicosIds: rev.servicos?.map((s) => s.id) || [],
              pecas: rev.pecas?.map((p) => ({ pecaId: p.id, quantidade: p.quantidade })) || [],
            }));

            const targetLinha = linhasData.find((l) => l.id === lid);
            const modelName = targetLinha ? `Plano Padrão — ${targetLinha.nome}` : 'Plano de Revisão';

            form.setFieldsValue({
              nome: modelName,
              linhaId: lid,
              quantidadeRevisoes: mappedRevisoes.length,
            });

            setLinhaId(lid);
            setRevisoes(mappedRevisoes);
          } else {
            form.setFieldsValue({ quantidadeRevisoes: 4 });
          }
        } else {
          form.setFieldsValue({ quantidadeRevisoes: 4 });
        }
      } catch (error) {
        handleApiError(error, 'Erro ao carregar dados para o cadastro de revisões.');
      } finally {
        setLoading(false);
      }
    };

    fetchDependencies();
  }, [form, paramLinhaId, isEditMode]);

  const modelosAtivosDaLinha = useMemo(
    () => modelos.filter((modelo) => modelo.linhaId == linhaId && modelo.ativo),
    [linhaId, modelos],
  );


  const linhaJaTemAtivo = useMemo(
    () => revisoesExistentes.some((r) => r.linhaId == linhaId && r.ativo),
    [revisoesExistentes, linhaId],
  );

  // Sync state if form changes lineId
  const watchLinhaId = Form.useWatch('linhaId', form);
  useEffect(() => {
    if (watchLinhaId !== undefined || currentStep === 0) {
      setLinhaId(watchLinhaId);
    }
  }, [watchLinhaId, currentStep]);

  // Sync state if form changes name
  const watchNome = Form.useWatch('nome', form);
  useEffect(() => {
    if (watchNome !== undefined || currentStep === 0) {
      setNomeModelo(watchNome || '');
    }
  }, [watchNome, currentStep]);

  const servicoById = useMemo(
    () => new Map(servicos.map((servico) => [servico.id, servico])),
    [servicos],
  );

  const pecaById = useMemo(
    () => new Map(pecas.map((peca) => [peca.id, peca])),
    [pecas],
  );

  const calcRevisao = (rev: RevisaoFormItem) => {
    const totalPecasValor = rev.pecas.reduce((s, p) => {
      const peca = pecaById.get(p.pecaId);
      return s + (peca ? peca.preco * p.quantidade : 0);
    }, 0);
    const totalServicosValor = rev.servicosIds.reduce((s, id) => {
      const servico = servicoById.get(id);
      return s + (servico ? servico.custo : 0);
    }, 0);
    const tempoMinutos = rev.servicosIds.reduce((s, id) => {
      const servico = servicoById.get(id);
      return s + (servico ? servico.tempoEstimado : 0);
    }, 0);
    return {
      totalPecasValor,
      totalServicosValor,
      tempoMinutos,
      valorTotal: totalPecasValor + totalServicosValor,
    };
  };

  const handleQuantidadeChange = (quantity: number) => {
    if (!isEditMode) {
      setRevisoes(createRevisoes(quantity));
    } else {
      // In edit mode only reset if no revisions loaded yet
      setRevisoes((prev) =>
        prev.length === 0 ? createRevisoes(quantity) : prev,
      );
    }
    setActiveRevTab('0');
  };

  const updateRevisaoField = (
    idx: number,
    field: 'nome' | 'quilometragem' | 'tempoMeses',
    value: string | number | null,
  ) => {
    setRevisoes((prev) =>
      prev.map((r, i) => {
        if (i !== idx) return r;
        const safeValue = value === null ? (field === 'nome' ? '' : 0) : value;
        return { ...r, [field]: safeValue };
      }),
    );
  };

  const addPeca = (revKey: string) => {
    if (!addPecaId) return;
    const qty = addPecaQty > 0 ? addPecaQty : 1;
    const newItem: RevisaoPadraoPecaRequest = {
      pecaId: addPecaId,
      quantidade: qty,
    };
    setRevisoes((prev) =>
      prev.map((r) =>
        r.key === revKey ? { ...r, pecas: [...r.pecas, newItem] } : r,
      ),
    );
    setAddPecaId(null);
    setAddPecaQty(1);
  };

  const removePeca = (revKey: string, pecaIdToRemove: number) => {
    setRevisoes((prev) =>
      prev.map((r) =>
        r.key === revKey ? { ...r, pecas: r.pecas.filter((p) => p.pecaId !== pecaIdToRemove) } : r,
      ),
    );
  };

  const addServico = (revKey: string) => {
    if (!addServId) return;
    setRevisoes((prev) =>
      prev.map((r) =>
        r.key === revKey ? { ...r, servicosIds: [...r.servicosIds, addServId] } : r,
      ),
    );
    setAddServId(null);
  };

  const removeServico = (revKey: string, idToRemove: number) => {
    setRevisoes((prev) =>
      prev.map((r) =>
        r.key === revKey ? { ...r, servicosIds: r.servicosIds.filter((id) => id !== idToRemove) } : r,
      ),
    );
  };

  const validateEstruturaRevisoes = () => {
    const invalidRevision = revisoes.find((revisao) => (
      !revisao.nome.trim()
      || revisao.quilometragem < 0
      || revisao.tempoMeses < 0
    ));

    if (invalidRevision) {
      message.error('Preencha nome, quilometragem e tempo em todas as revisões.');
      return false;
    }

    return true;
  };

  const validateServicosEPecas = () => {
    const invalidRevision = revisoes.find((revisao) => (
      revisao.servicosIds.length === 0
      || revisao.pecas.some((peca) => !peca.pecaId || peca.quantidade <= 0)
    ));

    if (invalidRevision) {
      message.error('Selecione ao menos um serviço e mantenha as peças válidas em todas as revisões.');
      return false;
    }

    return true;
  };

  const validateRevisoes = () => validateEstruturaRevisoes() && validateServicosEPecas();

  const goNext = async () => {
    if (currentStep === 0) {
      try {
        await form.validateFields(['nome', 'linhaId', 'quantidadeRevisoes']);
      } catch {
        return;
      }
    }

    if (currentStep === 1 && !validateEstruturaRevisoes()) {
      return;
    }

    if (currentStep === 2 && !validateServicosEPecas()) {
      return;
    }

    setCurrentStep((step) => step + 1);
  };

  const handleSave = async (status: 'ativo' | 'rascunho') => {
    if (status === 'ativo' && linhaJaTemAtivo && !isEditMode) {
      Modal.error({
        title: 'Não é possível publicar',
        content:
          'Esta linha já possui um Modelo de Revisão ativo. Desative o modelo existente antes de publicar um novo.',
      });
      return;
    }

    const performSave = async () => {
      try {
        const values = form.getFieldsValue();
        const finalNome = values.nome || nomeModelo;
        const finalLinhaId = values.linhaId || linhaId;

        if (!finalNome || !finalLinhaId) {
          message.error('Por favor, preencha os dados gerais do modelo na primeira etapa.');
          return;
        }

        if (!validateRevisoes()) return;

        const payload: RevisaoPadraoLinhaRequest = {
          nome: finalNome,
          linhaId: Number(finalLinhaId),
          revisoes: revisoes.map((revisao) => ({
            nome: revisao.nome,
            ordem: revisao.ordem,
            quilometragem: revisao.quilometragem,
            tempoMeses: revisao.tempoMeses,
            servicosIds: revisao.servicosIds,
            pecas: revisao.pecas.length > 0 ? revisao.pecas : undefined,
          })),
        };

        setSaving(true);
        if (isEditMode && paramLinhaId) {
          await revisaoPadraoService.atualizarPorLinha(Number(paramLinhaId), payload);
          message.success('Modelo de revisão atualizado com sucesso.');
        } else {
          await revisaoPadraoService.criarPorLinha(payload);
          message.success('Modelo de revisão cadastrado com sucesso.');
        }
        onBack();
      } catch (error) {
        handleApiError(error);
      } finally {
        setSaving(false);
      }
    };


    if (status === 'ativo') {
      const values = form.getFieldsValue();
      const currentLinhaId = values.linhaId || linhaId;
      const linha = linhas.find((l) => l.id == currentLinhaId);
      Modal.confirm({
        title: 'Publicar Modelo de Revisão',
        content: `Este Modelo de Revisão será aplicado automaticamente aos Modelos de Moto vinculados à Linha "${linha?.nome}".`,
        okText: 'Publicar',
        cancelText: 'Cancelar',
        onOk: performSave,
      });
    } else {
      Modal.confirm({
        title: 'Salvar como Rascunho',
        content: 'O servidor não suporta rascunhos. O modelo será publicado como ativo no sistema. Deseja prosseguir?',
        okText: 'Publicar',
        cancelText: 'Cancelar',
        onOk: performSave,
      });
    }
  };

  // ─── Step 1 ───────────────────────────────────────────────────────────────
  const renderStep1 = () => (
    <Card title="Dados Gerais">
      <div style={{ maxWidth: 600 }}>
        <Form.Item
          name="nome"
          label="Nome do Modelo"
          rules={[{ required: true, message: 'Informe o nome do modelo' }]}
        >
          <Input placeholder="Ex: Modelo Padrão - Passeio" />
        </Form.Item>

        <Form.Item
          name="linhaId"
          label="Linha de Moto"
          rules={[{ required: true, message: 'Selecione a linha' }]}
        >
          <Select
            placeholder="Selecione uma linha"
            style={{ width: '100%' }}
            disabled={isEditMode}
            onChange={(val: number) => {
              setLinhaId(val);
              const qty = form.getFieldValue('quantidadeRevisoes');
              if (qty) handleQuantidadeChange(qty);
            }}
            options={linhas.map((l) => ({ value: l.id, label: l.nome }))}
          />
        </Form.Item>

        {linhaId && (
          <Form.Item>
            {linhaJaTemAtivo && !isEditMode ? (
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
                message={`Linha ${linhas.find(l => l.id == linhaId)?.nome}: ${modelosAtivosDaLinha.length} modelo(s) de moto vinculado(s)`}
                description={
                  <Flex vertical gap="small">
                    {modelosAtivosDaLinha.slice(0, 5).map((m) => (
                      <Text key={m.id}>
                        {m.marca} {m.nomeModelo} — {m.ano}
                      </Text>
                    ))}
                    <Text>
                      Os Modelos de Moto vinculados à Linha {linhas.find(l => l.id == linhaId)?.nome} usarão este Modelo de
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
            disabled={isEditMode}
            onChange={(val: number) => handleQuantidadeChange(val)}
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
      </div>
    </Card>
  );

  // ─── Step 2 ───────────────────────────────────────────────────────────────
  const step2Columns: ColumnsType<RevisaoFormItem> = [
    {
      title: 'Nº Revisão',
      dataIndex: 'ordem',
      key: 'ordem',
      width: 90,
      render: (n: number) => ordinal(n),
    },
    {
      title: 'Nome da Revisão',
      dataIndex: 'nome',
      key: 'nome',
      render: (_, _record, idx: number) => (
        <Input
          value={revisoes[idx]?.nome}
          onChange={(e) => updateRevisaoField(idx, 'nome', e.target.value)}
          placeholder="Ex: Primeira revisão"
        />
      ),
    },
    {
      title: 'Quilometragem Máxima',
      dataIndex: 'quilometragem',
      key: 'quilometragem',
      render: (_, _record, idx: number) => (
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
      render: (_, _record, idx: number) => (
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
        rowKey="key"
        pagination={false}
        bordered
      />
    </Card>
  );

  // ─── Step 3 ───────────────────────────────────────────────────────────────
  const renderRevisaoTab = (rev: RevisaoFormItem) => {
    const { totalPecasValor, totalServicosValor, tempoMinutos, valorTotal } = calcRevisao(rev);

    const pecasJaAdicionadas = new Set(rev.pecas.map((p) => p.pecaId));
    const servicosJaAdicionados = new Set(rev.servicosIds);

    const pecasDisponiveis = pecas.filter((p) => !pecasJaAdicionadas.has(p.id));
    const servicosDisponiveis = servicos.filter((s) => !servicosJaAdicionados.has(s.id));

    const pecasColumns: ColumnsType<{ pecaId: number; quantidade: number }> = [
      {
        title: 'Código',
        key: 'codigo',
        width: 100,
        render: (_, record) => pecaById.get(record.pecaId)?.codigo || '-',
      },
      {
        title: 'Peça',
        key: 'nome',
        render: (_, record) => pecaById.get(record.pecaId)?.nome || '-',
      },
      {
        title: 'Qtd',
        dataIndex: 'quantidade',
        key: 'quantidade',
        width: 80,
        align: 'center',
      },
      {
        title: 'Valor Unitário',
        key: 'valorUnitario',
        width: 130,
        render: (_, record) => {
          const price = pecaById.get(record.pecaId)?.preco || 0;
          return formatCurrency(price);
        },
      },
      {
        title: 'Valor Total',
        key: 'valorTotal',
        width: 130,
        render: (_, record) => {
          const price = pecaById.get(record.pecaId)?.preco || 0;
          return formatCurrency(price * record.quantidade);
        },
      },
      {
        title: '',
        key: 'actions',
        width: 60,
        render: (_, record) => (
          <Button
            type="link"
            danger
            icon={<DeleteOutlined />}
            onClick={() => removePeca(rev.key, record.pecaId)}
          />
        ),
      },
    ];

    const servicosColumns: ColumnsType<number> = [
      {
        title: 'Serviço',
        key: 'nome',
        render: (_, id) => servicoById.get(id)?.nome || '-',
      },
      {
        title: 'Tempo Médio',
        key: 'tempoMinutos',
        width: 130,
        render: (_, id) => {
          const tempo = servicoById.get(id)?.tempoEstimado || 0;
          return formatTempo(tempo);
        },
      },
      {
        title: 'Valor Mão de Obra',
        key: 'valorMaoObra',
        width: 160,
        render: (_, id) => {
          const valor = servicoById.get(id)?.custo || 0;
          return formatCurrency(valor);
        },
      },
      {
        title: '',
        key: 'actions',
        width: 60,
        render: (_, id) => (
          <Button
            type="link"
            danger
            icon={<DeleteOutlined />}
            onClick={() => removeServico(rev.key, id)}
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
                      value={addPecaId || undefined}
                      onChange={(val) => setAddPecaId(val)}
                      options={pecasDisponiveis.map((p) => ({
                        value: p.id,
                        label: `${p.codigo} — ${p.nome} (${formatCurrency(p.preco)})`,
                      }))}
                      showSearch
                      optionFilterProp="label"
                    />
                    <InputNumber
                      min={1}
                      value={addPecaQty}
                      onChange={(val) => setAddPecaQty(val ?? 1)}
                      style={{ width: 80 }}
                    />
                    <Button
                      type="primary"
                      disabled={!addPecaId}
                      onClick={() => addPeca(rev.key)}
                    >
                      Adicionar
                    </Button>
                  </Flex>
                  <Table
                    dataSource={rev.pecas}
                    columns={pecasColumns}
                    rowKey="pecaId"
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
              label: `Serviços (${rev.servicosIds.length})`,
              children: (
                <Flex vertical gap="middle">
                  <Flex gap="middle" align="center">
                    <Select
                      style={{ flex: 1 }}
                      placeholder="Selecione um serviço"
                      value={addServId || undefined}
                      onChange={(val) => setAddServId(val)}
                      options={servicosDisponiveis.map((s) => ({
                        value: s.id,
                        label: `${s.nome} — ${s.tempoEstimado}min — ${formatCurrency(s.custo)}`,
                      }))}
                      showSearch
                      optionFilterProp="label"
                    />
                    <Button
                      type="primary"
                      disabled={!addServId}
                      onClick={() => addServico(rev.key)}
                    >
                      Adicionar
                    </Button>
                  </Flex>
                  <Table
                    dataSource={rev.servicosIds}
                    columns={servicosColumns}
                    rowKey={(id) => id}
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
          setAddPecaId(null);
          setAddPecaQty(1);
          setAddServId(null);
        }}
        items={revisoes.map((rev, idx) => ({
          key: String(idx),
          label: `${ordinal(rev.ordem)} Revisão`,
          children: renderRevisaoTab(rev),
        }))}
      />
    </Card>
  );

  // ─── Step 4 ───────────────────────────────────────────────────────────────
  const renderStep4 = () => {
    const values = form.getFieldsValue();
    const currentLinhaId = values.linhaId || linhaId;
    const selectedLinha = linhas.find((l) => l.id == currentLinhaId);
    const totalPecasUnicas = new Set(revisoes.flatMap((r) => r.pecas.map((p) => p.pecaId))).size;
    const totalServicosUnicos = new Set(revisoes.flatMap((r) => r.servicosIds)).size;
    const totalTempo = revisoes.reduce((s, r) => s + calcRevisao(r).tempoMinutos, 0);
    const totalValor = revisoes.reduce((s, r) => s + calcRevisao(r).valorTotal, 0);

    const resumoColumns: ColumnsType<RevisaoFormItem> = [
      {
        title: 'Nº Revisão',
        dataIndex: 'ordem',
        key: 'ordem',
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
        render: (_, record) => record.pecas.length,
      },
      {
        title: 'Serviços',
        key: 'servicos',
        width: 90,
        align: 'center',
        render: (_, record) => record.servicosIds.length,
      },
      {
        title: 'Tempo Estimado',
        key: 'tempo',
        width: 130,
        render: (_, record) =>
          formatTempo(calcRevisao(record).tempoMinutos),
      },
      {
        title: 'Valor Médio',
        key: 'valor',
        width: 130,
        render: (_, record) =>
          formatCurrency(calcRevisao(record).valorTotal),
      },
    ];

    return (
      <Flex vertical gap="large">
        {linhaJaTemAtivo && !isEditMode && (
          <Alert
            type="warning"
            showIcon
            message="Publicação bloqueada"
            description="Esta linha já possui um Modelo de Revisão ativo. Você só pode salvar como Rascunho."
          />
        )}

        <Card title="Resumo do Modelo">
          <Descriptions bordered column={2}>
            <Descriptions.Item label="Nome do Modelo">{values.nome || nomeModelo || '-'}</Descriptions.Item>
            <Descriptions.Item label="Linha de Moto">{selectedLinha?.nome || '-'}</Descriptions.Item>
            <Descriptions.Item label="Quantidade de Revisões">
              {values.quantidadeRevisoes || '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Modelos de Moto Herdeiros">
              {modelosAtivosDaLinha.length} modelo(s)
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
            rowKey="key"
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
    <Spin spinning={loading || saving}>
      <Form form={form} layout="vertical" preserve={true}>
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
                { title: isEditMode ? 'Editar Modelo de Revisão' : 'Novo Modelo de Revisão' },
              ]}
            />
            <Flex gap="middle" align="center">
              <Button icon={<ArrowLeftOutlined />} onClick={onBack} />
              <Title level={2} style={{ margin: 0 }}>
                {isEditMode ? 'Editar Modelo de Revisão' : 'Novo Modelo de Revisão'}
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
                      disabled={linhaJaTemAtivo && !isEditMode}
                      onClick={() => handleSave('ativo')}
                    >
                      {isEditMode ? 'Salvar Modelo de Revisão' : 'Publicar Modelo de Revisão'}
                    </Button>
                  </>
                )}
              </Flex>
            </Flex>
          </Card>
        </Flex>
      </Form>
    </Spin>
  );
}
