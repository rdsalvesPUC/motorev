import { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Breadcrumb,
  Button,
  Card,
  Flex,
  Form,
  Input,
  InputNumber,
  message,
  Modal,
  Select,
  Space,
  Switch,
  Table,
  Tag,
  Typography,
} from 'antd';
import { EditOutlined, HomeOutlined, ReloadOutlined, SearchOutlined, ToolOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useNavigate } from 'react-router';
import {
  CATEGORIAS_PECA,
  pecaService,
  type CategoriaPeca,
  type PecaResponse,
  type PecaUpdateRequest,
  type StatusCadastro,
  type StatusPecaFilter,
} from '../../services/pecaService';
import { PATHS } from '../../paths';

const { Title, Text } = Typography;

type EstoqueFilter = 'disponivel' | 'baixo' | 'zerado';

interface PecaFormValues {
  codigo: string;
  nome: string;
  categoria: CategoriaPeca;
  preco: number;
  estoque: number;
}

interface EditableCellProps {
  editing: boolean;
  dataIndex: keyof PecaFormValues;
  record?: PecaResponse;
  title: string;
  children: React.ReactNode;
}

const EditableCell: React.FC<EditableCellProps> = ({
  editing,
  dataIndex,
  record: _record,
  title,
  children,
  ...restProps
}) => {
  let inputNode: React.ReactNode;

  if (dataIndex === 'categoria') {
    inputNode = <Select options={CATEGORIAS_PECA} />;
  } else if (dataIndex === 'preco') {
    inputNode = (
      <InputNumber
        addonBefore="R$"
        decimalSeparator=","
        min={0.01}
        precision={2}
        step={0.01}
        style={{ width: '100%' }}
      />
    );
  } else if (dataIndex === 'estoque') {
    inputNode = <InputNumber min={0} precision={0} style={{ width: '100%' }} />;
  } else {
    inputNode = <Input />;
  }

  return (
    <td {...restProps}>
      {editing ? (
        <Form.Item
          name={dataIndex}
          style={{ margin: 0 }}
          rules={[
            {
              required: true,
              message: `Por favor, insira ${title}`,
            },
          ]}
        >
          {inputNode}
        </Form.Item>
      ) : (
        children
      )}
    </td>
  );
};

function formatCurrency(value: number) {
  return value.toLocaleString('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  });
}

function getEstoqueTag(estoque: number) {
  const color = estoque > 10 ? 'green' : estoque > 0 ? 'orange' : 'red';
  return <Tag color={color}>{estoque} unidades</Tag>;
}

function getStatusTag(status: string) {
  return <Tag color={status === 'Ativo' ? 'green' : 'red'}>{status}</Tag>;
}

function toUpdateRequest(peca: PecaResponse, values?: Partial<PecaFormValues>): PecaUpdateRequest {
  return {
    codigo: values?.codigo ?? peca.codigo,
    nome: values?.nome ?? peca.nome,
    categoria: (values?.categoria ?? peca.categoria) as CategoriaPeca,
    preco: Number(values?.preco ?? peca.preco),
    estoque: Number(values?.estoque ?? peca.estoque),
    status: peca.status as StatusCadastro,
  };
}

export default function CatalogoPecas() {
  const navigate = useNavigate();
  const [form] = Form.useForm<PecaFormValues>();
  const [pecas, setPecas] = useState<PecaResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [savingKey, setSavingKey] = useState<number | null>(null);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [categoria, setCategoria] = useState<string>();
  const [estoque, setEstoque] = useState<EstoqueFilter>();
  const [status, setStatus] = useState<StatusPecaFilter>('Ativo');
  const [editingKey, setEditingKey] = useState<number | null>(null);

  const loadPecas = async () => {
    setLoading(true);
    setError('');

    try {
      const data = await pecaService.listar(status);
      setPecas(data);
    } catch (err: any) {
      const errorMessage = err.message || 'Não foi possível carregar o catálogo de peças.';
      setError(errorMessage);
      message.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPecas();
  }, [status]);

  const filteredPecas = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    return pecas.filter((peca) => {
      const matchesSearch =
        !normalizedSearch ||
        peca.codigo.toLowerCase().includes(normalizedSearch) ||
        peca.nome.toLowerCase().includes(normalizedSearch);

      const matchesCategoria = !categoria || peca.categoria === categoria;

      const matchesEstoque =
        !estoque ||
        (estoque === 'disponivel' && peca.estoque > 10) ||
        (estoque === 'baixo' && peca.estoque > 0 && peca.estoque <= 10) ||
        (estoque === 'zerado' && peca.estoque === 0);

      return matchesSearch && matchesCategoria && matchesEstoque;
    });
  }, [pecas, search, categoria, estoque]);

  const clearFilters = () => {
    setSearch('');
    setCategoria(undefined);
    setEstoque(undefined);
    setStatus('Ativo');
  };

  const isEditing = (record: PecaResponse) => record.id === editingKey;

  const edit = (record: PecaResponse) => {
    form.setFieldsValue({
      codigo: record.codigo,
      nome: record.nome,
      categoria: record.categoria as CategoriaPeca,
      preco: record.preco,
      estoque: record.estoque,
    });
    setEditingKey(record.id);
  };

  const cancel = () => {
    Modal.confirm({
      title: 'Cancelar edição',
      content: 'Tem certeza que deseja cancelar as alterações?',
      okText: 'Sim',
      cancelText: 'Não',
      onOk() {
        form.resetFields();
        setEditingKey(null);
      },
    });
  };

  const save = async (record: PecaResponse) => {
    try {
      const values = await form.validateFields();

      Modal.confirm({
        title: 'Salvar alterações',
        content: 'Tem certeza que deseja salvar as alterações?',
        okText: 'Sim',
        cancelText: 'Não',
        async onOk() {
          setSavingKey(record.id);

          try {
            const updatedPeca = await pecaService.atualizar(record.id, toUpdateRequest(record, values));
            setPecas((currentPecas) =>
              currentPecas.map((peca) => (peca.id === record.id ? updatedPeca : peca)),
            );
            form.resetFields();
            setEditingKey(null);
            message.success('Peça atualizada com sucesso.');
          } catch (err: any) {
            message.error(err.message || 'Não foi possível atualizar a peça.');
          } finally {
            setSavingKey(null);
          }
        },
      });
    } catch {
      message.error('Verifique os campos antes de salvar.');
    }
  };

  const toggleStatus = async (record: PecaResponse) => {
    const nextStatus: StatusCadastro = record.status === 'Ativo' ? 'Inativo' : 'Ativo';

    const updateStatus = async () => {
      setSavingKey(record.id);

      try {
        const updatedPeca = await pecaService.atualizarStatus(record.id, { status: nextStatus });
        setPecas((currentPecas) =>
          currentPecas.map((peca) => (peca.id === record.id ? updatedPeca : peca)),
        );
        message.success(nextStatus === 'Ativo' ? 'Peça ativada com sucesso.' : 'Peça inativada com sucesso.');
      } catch (err: any) {
        message.error(err.message || 'Não foi possível alterar o status da peça.');
      } finally {
        setSavingKey(null);
      }
    };

    if (nextStatus === 'Inativo') {
      Modal.confirm({
        title: 'Inativar peça',
        content: 'Tem certeza que deseja inativar esta peça?',
        okText: 'Sim',
        cancelText: 'Não',
        onOk: updateStatus,
      });
      return;
    }

    await updateStatus();
  };

  const emptyText =
    pecas.length === 0
      ? 'Nenhuma peça cadastrada.'
      : 'Nenhuma peça encontrada com os filtros selecionados.';

  const columns: ColumnsType<PecaResponse> = [
    {
      title: 'Código',
      dataIndex: 'codigo',
      key: 'codigo',
      sorter: (a, b) => a.codigo.localeCompare(b.codigo),
      onCell: (record) => ({
        record,
        dataIndex: 'codigo',
        title: 'Código',
        editing: isEditing(record),
      }),
    },
    {
      title: 'Nome da Peça',
      dataIndex: 'nome',
      key: 'nome',
      sorter: (a, b) => a.nome.localeCompare(b.nome),
      onCell: (record) => ({
        record,
        dataIndex: 'nome',
        title: 'Nome da Peça',
        editing: isEditing(record),
      }),
    },
    {
      title: 'Categoria',
      dataIndex: 'categoria',
      key: 'categoria',
      filters: CATEGORIAS_PECA.map((item) => ({ text: item.label, value: item.value })),
      onFilter: (value, record) => record.categoria === value,
      onCell: (record) => ({
        record,
        dataIndex: 'categoria',
        title: 'Categoria',
        editing: isEditing(record),
      }),
    },
    {
      title: 'Preço',
      dataIndex: 'preco',
      key: 'preco',
      align: 'right',
      render: (preco: number) => formatCurrency(preco),
      sorter: (a, b) => a.preco - b.preco,
      onCell: (record) => ({
        record,
        dataIndex: 'preco',
        title: 'Preço',
        editing: isEditing(record),
      }),
    },
    {
      title: 'Estoque',
      dataIndex: 'estoque',
      key: 'estoque',
      render: getEstoqueTag,
      sorter: (a, b) => a.estoque - b.estoque,
      onCell: (record) => ({
        record,
        dataIndex: 'estoque',
        title: 'Estoque',
        editing: isEditing(record),
      }),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 140,
      align: 'center',
      render: (_status: string, record: PecaResponse) =>
        isEditing(record) ? (
          getStatusTag(record.status)
        ) : (
          <Switch
            checked={record.status === 'Ativo'}
            checkedChildren="Ativo"
            disabled={editingKey !== null || savingKey === record.id}
            loading={savingKey === record.id}
            onChange={() => toggleStatus(record)}
            unCheckedChildren="Inativo"
          />
        ),
    },
    {
      title: 'Ações',
      key: 'actions',
      width: 1,
      render: (_: unknown, record: PecaResponse) => {
        const editable = isEditing(record);
        return editable ? (
          <Space>
            <Button type="link" loading={savingKey === record.id} onClick={() => save(record)}>
              Salvar
            </Button>
            <Button type="link" danger onClick={cancel}>
              Cancelar
            </Button>
          </Space>
        ) : (
          <Button
            type="link"
            icon={<EditOutlined />}
            disabled={editingKey !== null}
            onClick={() => edit(record)}
          >
            Editar
          </Button>
        );
      },
    },
  ];

  return (
    <Flex vertical gap="large" style={{ width: '100%' }}>
      <Flex vertical gap="middle" style={{ width: '100%' }}>
        <Breadcrumb
          items={[
            {
              title: <HomeOutlined />,
            },
            {
              title: (
                <>
                  <ToolOutlined />
                  <span>Catálogos</span>
                </>
              ),
            },
            {
              title: 'Peças',
            },
          ]}
        />

        <Title level={2} style={{ margin: 0 }}>
          Peças
        </Title>

        <Flex gap="middle" align="center" wrap="wrap">
          <Input
            allowClear
            placeholder="Buscar peças..."
            prefix={<SearchOutlined />}
            style={{ width: 300 }}
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />

          <Select
            allowClear
            options={CATEGORIAS_PECA}
            placeholder="Categoria"
            style={{ width: 160 }}
            value={categoria}
            onChange={setCategoria}
          />

          <Select
            allowClear
            options={[
              { value: 'disponivel', label: 'Disponível' },
              { value: 'baixo', label: 'Estoque Baixo' },
              { value: 'zerado', label: 'Sem Estoque' },
            ]}
            placeholder="Estoque"
            style={{ width: 160 }}
            value={estoque}
            onChange={setEstoque}
          />

          <Select
            options={[
              { value: 'Ativo', label: 'Ativas' },
              { value: 'Inativo', label: 'Inativas' },
              { value: 'Todos', label: 'Todas' },
            ]}
            placeholder="Status"
            style={{ width: 140 }}
            value={status}
            onChange={setStatus}
          />

          <Flex gap="small" style={{ marginLeft: 'auto' }}>
            <Button onClick={clearFilters}>Limpar</Button>
            <Button icon={<ReloadOutlined />} loading={loading} onClick={loadPecas}>
              Atualizar
            </Button>
          </Flex>
        </Flex>
      </Flex>

      {error && <Alert type="error" message={error} showIcon />}

      <Card>
        <Flex vertical gap="middle" style={{ width: '100%' }}>
          <Flex justify="space-between" align="center" wrap="wrap" gap="middle">
            <Text>Total: {filteredPecas.length} peças</Text>
            <Button
              type="primary"
              disabled={editingKey !== null}
              onClick={() => navigate(PATHS.CONCESSIONARIA_CATALOGOS_PECAS_CREATE)}
            >
              Adicionar Peça
            </Button>
          </Flex>

          <Form form={form} component={false}>
            <Table
              bordered
              components={{
                body: {
                  cell: EditableCell,
                },
              }}
              columns={columns}
              dataSource={filteredPecas}
              loading={loading}
              locale={{ emptyText }}
              pagination={{ pageSize: 10, showSizeChanger: true }}
              rowKey="id"
              rowClassName="editable-row"
            />
          </Form>
        </Flex>
      </Card>
    </Flex>
  );
}
