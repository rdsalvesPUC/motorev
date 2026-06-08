import { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  Card,
  Descriptions,
  Empty,
  Flex,
  Form,
  Input,
  InputNumber,
  Select,
  Space,
  Spin,
  Steps,
  Table,
  Tabs,
  Tag,
  Typography,
  message,
} from 'antd';
import { ArrowLeftOutlined, DeleteOutlined, PlusOutlined, ToolOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
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

const createRevisoes = (quantity: number): RevisaoFormItem[] =>
  DEFAULT_REVISAO_VALUES.slice(0, quantity).map((item) => ({
    ...item,
    key: item.ordem.toString(),
    servicosIds: [],
    pecas: [],
  }));

export default function CatalogoModelosRevisaoCreate({ onBack }: CatalogoModelosRevisaoCreateProps) {
  const [form] = Form.useForm();
  const [currentStep, setCurrentStep] = useState(0);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [linhas, setLinhas] = useState<Linha[]>([]);
  const [modelos, setModelos] = useState<ModeloMoto[]>([]);
  const [servicos, setServicos] = useState<Servico[]>([]);
  const [pecas, setPecas] = useState<PecaResponse[]>([]);
  const [linhaId, setLinhaId] = useState<number | undefined>();
  const [revisoes, setRevisoes] = useState<RevisaoFormItem[]>(createRevisoes(4));
  const [activeRevTab, setActiveRevTab] = useState('0');

  useEffect(() => {
    const fetchDependencies = async () => {
      try {
        setLoading(true);
        const [linhasData, modelosData, servicosData, pecasData] = await Promise.all([
          linhaService.getAll(true),
          modeloMotoService.getAll(),
          servicoService.getAll(),
          pecaService.listar('Ativo'),
        ]);

        setLinhas(linhasData);
        setModelos(modelosData);
        setServicos(servicosData);
        setPecas(pecasData);
        form.setFieldsValue({ quantidadeRevisoes: 4 });
      } catch (error) {
        handleApiError(error, 'Erro ao carregar dados para o cadastro de revisões.');
      } finally {
        setLoading(false);
      }
    };

    fetchDependencies();
  }, [form]);

  const modelosAtivosDaLinha = useMemo(
    () => modelos.filter((modelo) => modelo.linhaId === linhaId && modelo.ativo),
    [linhaId, modelos],
  );

  const servicoById = useMemo(
    () => new Map(servicos.map((servico) => [servico.id, servico])),
    [servicos],
  );

  const pecaById = useMemo(
    () => new Map(pecas.map((peca) => [peca.id, peca])),
    [pecas],
  );

  const updateRevisao = (key: string, patch: Partial<RevisaoFormItem>) => {
    setRevisoes((prev) => prev.map((revisao) => revisao.key === key ? { ...revisao, ...patch } : revisao));
  };

  const updatePeca = (revisaoKey: string, index: number, patch: Partial<RevisaoPadraoPecaRequest>) => {
    setRevisoes((prev) => prev.map((revisao) => {
      if (revisao.key !== revisaoKey) return revisao;

      const pecasAtualizadas = revisao.pecas.map((peca, pecaIndex) => (
        pecaIndex === index ? { ...peca, ...patch } : peca
      ));

      return { ...revisao, pecas: pecasAtualizadas };
    }));
  };

  const addPeca = (revisaoKey: string) => {
    setRevisoes((prev) => prev.map((revisao) => (
      revisao.key === revisaoKey
        ? { ...revisao, pecas: [...revisao.pecas, { pecaId: 0, quantidade: 1 }] }
        : revisao
    )));
  };

  const removePeca = (revisaoKey: string, index: number) => {
    setRevisoes((prev) => prev.map((revisao) => (
      revisao.key === revisaoKey
        ? { ...revisao, pecas: revisao.pecas.filter((_, pecaIndex) => pecaIndex !== index) }
        : revisao
    )));
  };

  const handleQuantidadeChange = (quantity: number) => {
    setRevisoes(createRevisoes(quantity));
    setActiveRevTab('0');
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

    if (modelosAtivosDaLinha.length === 0) {
      message.error('A linha selecionada não possui modelos de moto ativos.');
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
        await form.validateFields();
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

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      if (!validateRevisoes()) return;

      const payload: RevisaoPadraoLinhaRequest = {
        nome: values.nome,
        linhaId: values.linhaId,
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
      await revisaoPadraoService.criarPorLinha(payload);
      message.success('Modelo de revisão cadastrado com sucesso.');
      onBack();
    } catch (error) {
      handleApiError(error);
    } finally {
      setSaving(false);
    }
  };

  const revisaoColumns: ColumnsType<RevisaoFormItem> = [
    {
      title: 'Ordem',
      dataIndex: 'ordem',
      key: 'ordem',
      width: 90,
      render: (ordem: number) => `${ordem}ª`,
    },
    {
      title: 'Nome',
      dataIndex: 'nome',
      key: 'nome',
      render: (_: string, record) => (
        <Input
          value={record.nome}
          onChange={(event) => updateRevisao(record.key, { nome: event.target.value })}
        />
      ),
    },
    {
      title: 'Quilometragem',
      dataIndex: 'quilometragem',
      key: 'quilometragem',
      width: 180,
      render: (_: number, record) => (
        <InputNumber
          min={0}
          precision={0}
          addonAfter="km"
          style={{ width: '100%' }}
          value={record.quilometragem}
          parser={(value) => value?.replace(/[^\d]/g, '') as any}
          onChange={(value) => updateRevisao(record.key, { quilometragem: value ?? 0 })}
        />
      ),
    },
    {
      title: 'Tempo',
      dataIndex: 'tempoMeses',
      key: 'tempoMeses',
      width: 160,
      render: (_: number, record) => (
        <InputNumber
          min={0}
          precision={0}
          addonAfter="meses"
          style={{ width: '100%' }}
          value={record.tempoMeses}
          parser={(value) => value?.replace(/[^\d]/g, '') as any}
          onChange={(value) => updateRevisao(record.key, { tempoMeses: value ?? 0 })}
        />
      ),
    },
  ];

  const renderStep1 = () => (
    <Card title="Dados Gerais">
      <Form form={form} layout="vertical" style={{ maxWidth: 720 }}>
        <Form.Item
          name="nome"
          label="Nome do Modelo de Revisão"
          rules={[{ required: true, message: 'Informe o nome do modelo de revisão.' }]}
        >
          <Input placeholder="Ex: Plano padrão Street" />
        </Form.Item>

        <Form.Item
          name="linhaId"
          label="Linha de Moto"
          rules={[{ required: true, message: 'Selecione a linha.' }]}
        >
          <Select
            placeholder="Selecione uma linha"
            onChange={(value) => setLinhaId(value)}
            options={linhas.map((linha) => ({ value: linha.id, label: linha.nome }))}
          />
        </Form.Item>

        {linhaId && (
          <Alert
            showIcon
            type={modelosAtivosDaLinha.length > 0 ? 'info' : 'warning'}
            message={`${modelosAtivosDaLinha.length} modelo(s) ativo(s) vinculado(s) à linha selecionada`}
            description={
              modelosAtivosDaLinha.length > 0
                ? modelosAtivosDaLinha.map((modelo) => `${modelo.marca} ${modelo.nomeModelo}`).join(', ')
                : 'Cadastre ou ative ao menos um modelo de moto nesta linha antes de criar revisões.'
            }
            style={{ marginBottom: 24 }}
          />
        )}

        <Form.Item
          name="quantidadeRevisoes"
          label="Quantidade de Revisões"
          rules={[{ required: true, message: 'Selecione a quantidade de revisões.' }]}
        >
          <Select
            options={[
              { value: 4, label: '4 revisões' },
              { value: 7, label: '7 revisões' },
            ]}
            onChange={handleQuantidadeChange}
          />
        </Form.Item>
      </Form>
    </Card>
  );

  const renderStep2 = () => (
    <Card title="Estrutura das Revisões">
      <Table
        dataSource={revisoes}
        columns={revisaoColumns}
        rowKey="key"
        pagination={false}
        bordered
      />
    </Card>
  );

  const renderServicosEPecas = (revisao: RevisaoFormItem) => {
    const columns: ColumnsType<RevisaoPadraoPecaRequest & { index: number }> = [
      {
        title: 'Peça',
        dataIndex: 'pecaId',
        key: 'pecaId',
        render: (_: number, record) => (
          <Select
            placeholder="Selecione uma peça"
            value={record.pecaId || undefined}
            style={{ width: '100%' }}
            onChange={(pecaId) => updatePeca(revisao.key, record.index, { pecaId })}
            options={pecas.map((peca) => ({
              value: peca.id,
              label: `${peca.codigo} - ${peca.nome} (${formatCurrency(peca.preco)})`,
            }))}
          />
        ),
      },
      {
        title: 'Quantidade',
        dataIndex: 'quantidade',
        key: 'quantidade',
        width: 140,
        render: (_: number, record) => (
          <InputNumber
            min={1}
            precision={0}
            value={record.quantidade}
            style={{ width: '100%' }}
            parser={(value) => value?.replace(/[^\d]/g, '') as any}
            onChange={(quantidade) => updatePeca(revisao.key, record.index, { quantidade: quantidade ?? 1 })}
          />
        ),
      },
      {
        title: 'Total',
        key: 'total',
        width: 140,
        render: (_: any, record) => {
          const peca = pecaById.get(record.pecaId);
          return peca ? formatCurrency(peca.preco * record.quantidade) : '-';
        },
      },
      {
        title: '',
        key: 'actions',
        width: 60,
        render: (_: any, record) => (
          <Button
            type="link"
            danger
            icon={<DeleteOutlined />}
            onClick={() => removePeca(revisao.key, record.index)}
          />
        ),
      },
    ];

    return (
      <Space direction="vertical" size="middle" style={{ width: '100%' }}>
        <Descriptions size="small" bordered>
          <Descriptions.Item label="Quilometragem">
            {revisao.quilometragem.toLocaleString('pt-BR')} km
          </Descriptions.Item>
          <Descriptions.Item label="Tempo">{revisao.tempoMeses} meses</Descriptions.Item>
        </Descriptions>

        <Card size="small" title="Serviços">
          <Select
            mode="multiple"
            placeholder="Selecione os serviços desta revisão"
            value={revisao.servicosIds}
            onChange={(servicosIds) => updateRevisao(revisao.key, { servicosIds })}
            options={servicos.map((servico) => ({ value: servico.id, label: servico.nome }))}
            style={{ width: '100%' }}
          />
        </Card>

        <Card size="small" title="Peças">
          <Flex justify="space-between" align="center">
            <Text>Peças opcionais da {revisao.ordem}ª revisão</Text>
            <Button icon={<PlusOutlined />} onClick={() => addPeca(revisao.key)}>
              Adicionar Peça
            </Button>
          </Flex>

          <Table
            size="small"
            bordered
            pagination={false}
            rowKey="index"
            dataSource={revisao.pecas.map((peca, index) => ({ ...peca, index }))}
            columns={columns}
            locale={{
              emptyText: (
                <Empty
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description="Nenhuma peça adicionada."
                />
              ),
            }}
          />
        </Card>
      </Space>
    );
  };

  const renderStep3 = () => (
    <Card title="Peças e Serviços por Revisão">
      <Tabs
        activeKey={activeRevTab}
        onChange={setActiveRevTab}
        items={revisoes.map((revisao, index) => ({
          key: index.toString(),
          label: `${revisao.ordem}ª Revisão`,
          children: renderServicosEPecas(revisao),
        }))}
      />
    </Card>
  );

  const renderStep4 = () => {
    const values = form.getFieldsValue();
    const linha = linhas.find((item) => item.id === values.linhaId);
    const servicosUnicos = new Set(revisoes.flatMap((revisao) => revisao.servicosIds));
    const pecasUnicas = new Set(revisoes.flatMap((revisao) => revisao.pecas.map((peca) => peca.pecaId).filter(Boolean)));

    return (
      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        <Card title="Resumo do Modelo">
          <Descriptions bordered column={2}>
            <Descriptions.Item label="Nome">{values.nome || '-'}</Descriptions.Item>
            <Descriptions.Item label="Linha">{linha?.nome || '-'}</Descriptions.Item>
            <Descriptions.Item label="Modelos vinculados">{modelosAtivosDaLinha.length}</Descriptions.Item>
            <Descriptions.Item label="Revisões">{revisoes.length}</Descriptions.Item>
            <Descriptions.Item label="Serviços únicos">{servicosUnicos.size}</Descriptions.Item>
            <Descriptions.Item label="Peças únicas">{pecasUnicas.size}</Descriptions.Item>
            <Descriptions.Item label="Status">
              <Tag color="green">Ativo</Tag>
            </Descriptions.Item>
          </Descriptions>
        </Card>

        <Card title="Revisões">
          <Table
            size="small"
            bordered
            pagination={false}
            rowKey="key"
            dataSource={revisoes}
            columns={[
              { title: 'Ordem', dataIndex: 'ordem', key: 'ordem', width: 90, render: (ordem: number) => `${ordem}ª` },
              { title: 'Nome', dataIndex: 'nome', key: 'nome' },
              { title: 'KM', dataIndex: 'quilometragem', key: 'quilometragem', render: (value: number) => `${value.toLocaleString('pt-BR')} km` },
              { title: 'Tempo', dataIndex: 'tempoMeses', key: 'tempoMeses', render: (value: number) => `${value} meses` },
              {
                title: 'Serviços',
                dataIndex: 'servicosIds',
                key: 'servicosIds',
                render: (ids: number[]) => ids.map((id) => servicoById.get(id)?.nome).filter(Boolean).join(', '),
              },
              {
                title: 'Peças',
                dataIndex: 'pecas',
                key: 'pecas',
                render: (items: RevisaoPadraoPecaRequest[]) => items.length,
                width: 90,
                align: 'center',
              },
            ]}
          />
        </Card>
      </Space>
    );
  };

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
      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        <Space direction="vertical" size="middle">
          <DashboardBreadcrumb
            userType="concessionaria"
            items={[
              {
                title: 'Modelos de Revisão',
                icon: <ToolOutlined />,
              },
              {
                title: 'Novo Modelo de Revisão',
              },
            ]}
          />

          <Flex gap="middle" align="center">
            <Button icon={<ArrowLeftOutlined />} onClick={onBack} />
            <Title level={2} style={{ margin: 0 }}>
              Novo Modelo de Revisão
            </Title>
          </Flex>
        </Space>

        <Steps
          current={currentStep}
          items={[
            { title: 'Dados Gerais' },
            { title: 'Revisões' },
            { title: 'Peças e Serviços' },
            { title: 'Resumo' },
          ]}
        />

        {renderCurrentStep()}

        <Card>
          <Flex justify="space-between" align="center">
            <Button onClick={onBack}>Cancelar</Button>
            <Space>
              {currentStep > 0 && (
                <Button onClick={() => setCurrentStep((step) => step - 1)}>Voltar</Button>
              )}
              {currentStep < 3 && (
                <Button type="primary" onClick={goNext}>
                  Próximo
                </Button>
              )}
              {currentStep === 3 && (
                <Button type="primary" loading={saving} onClick={handleSubmit}>
                  Salvar Modelo de Revisão
                </Button>
              )}
            </Space>
          </Flex>
        </Card>
      </Space>
    </Spin>
  );
}
