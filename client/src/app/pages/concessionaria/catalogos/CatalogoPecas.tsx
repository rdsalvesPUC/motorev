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
} from '@/app/services/pecaService';
import { PATHS } from '@/app/paths';
import { t } from '@/app/i18n';
import { formatCurrency, getCurrencySymbol, getDecimalSeparator } from '@/app/utils/formatters';

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

const categoriaPecaKeys: Record<CategoriaPeca, string> = {
  Filtros: 'partsCatalog.category.filters',
  Motor: 'partsCatalog.category.engine',
  Freios: 'partsCatalog.category.brakes',
  Transmissão: 'partsCatalog.category.transmission',
  Elétrica: 'partsCatalog.category.electrical',
};

function getCategoriaPecaOptions() {
  return CATEGORIAS_PECA.map(({ value }) => ({
    value,
    label: t(categoriaPecaKeys[value]),
  }));
}

function translateCategoria(categoria: string) {
  return categoriaPecaKeys[categoria as CategoriaPeca]
    ? t(categoriaPecaKeys[categoria as CategoriaPeca])
    : categoria;
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
    inputNode = <Select options={getCategoriaPecaOptions()} />;
  } else if (dataIndex === 'preco') {
    inputNode = (
      <InputNumber
        addonBefore={getCurrencySymbol()}
        decimalSeparator={getDecimalSeparator()}
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
              message: t('partsCatalog.enterField', { field: title }),
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

function getEstoqueTag(estoque: number) {
  const color = estoque > 10 ? 'green' : estoque > 0 ? 'orange' : 'red';
  return <Tag color={color}>{t('partsCatalog.stockUnits', { count: estoque })}</Tag>;
}

function getStatusTag(status: string) {
  return (
    <Tag color={status === 'Ativo' ? 'green' : 'red'}>
      {status === 'Ativo' ? t('partsCatalog.status.active') : t('partsCatalog.status.inactive')}
    </Tag>
  );
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
      const errorMessage = err.message || t('partsCatalog.error.load');
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
      title: t('partsCatalog.cancelEdit.title'),
      content: t('partsCatalog.cancelEdit.content'),
      okText: t('yes'),
      cancelText: t('no'),
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
        title: t('partsCatalog.saveEdit.title'),
        content: t('partsCatalog.saveEdit.content'),
        okText: t('yes'),
        cancelText: t('no'),
        async onOk() {
          setSavingKey(record.id);

          try {
            const updatedPeca = await pecaService.atualizar(record.id, toUpdateRequest(record, values));
            setPecas((currentPecas) =>
              currentPecas.map((peca) => (peca.id === record.id ? updatedPeca : peca)),
            );
            form.resetFields();
            setEditingKey(null);
            message.success(t('partsCatalog.update.success'));
          } catch (err: any) {
            message.error(err.message || t('partsCatalog.update.error'));
          } finally {
            setSavingKey(null);
          }
        },
      });
    } catch {
      message.error(t('partsCatalog.save.validationError'));
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
        message.success(
          nextStatus === 'Ativo' ? t('partsCatalog.activate.success') : t('partsCatalog.deactivate.success'),
        );
      } catch (err: any) {
        message.error(err.message || t('partsCatalog.status.error'));
      } finally {
        setSavingKey(null);
      }
    };

    if (nextStatus === 'Inativo') {
      Modal.confirm({
        title: t('partsCatalog.deactivate.title'),
        content: t('partsCatalog.deactivate.content'),
        okText: t('yes'),
        cancelText: t('no'),
        onOk: updateStatus,
      });
      return;
    }

    await updateStatus();
  };

  const emptyText =
    pecas.length === 0
      ? t('partsCatalog.empty.noData')
      : t('partsCatalog.empty.filtered');

  const columns: ColumnsType<PecaResponse> = [
    {
      title: t('partsCatalog.code'),
      dataIndex: 'codigo',
      key: 'codigo',
      sorter: (a, b) => a.codigo.localeCompare(b.codigo),
      onCell: (record) => ({
        record,
        dataIndex: 'codigo',
        title: t('partsCatalog.code'),
        editing: isEditing(record),
      }),
    },
    {
      title: t('partsCatalog.partName'),
      dataIndex: 'nome',
      key: 'nome',
      sorter: (a, b) => a.nome.localeCompare(b.nome),
      onCell: (record) => ({
        record,
        dataIndex: 'nome',
        title: t('partsCatalog.partName'),
        editing: isEditing(record),
      }),
    },
    {
      title: t('partsCatalog.category'),
      dataIndex: 'categoria',
      key: 'categoria',
      filters: getCategoriaPecaOptions().map((item) => ({ text: item.label, value: item.value })),
      onFilter: (value, record) => record.categoria === value,
      render: (categoriaValue: string) => translateCategoria(categoriaValue),
      onCell: (record) => ({
        record,
        dataIndex: 'categoria',
        title: t('partsCatalog.category'),
        editing: isEditing(record),
      }),
    },
    {
      title: t('partsCatalog.price'),
      dataIndex: 'preco',
      key: 'preco',
      align: 'right',
      render: (preco: number) => formatCurrency(preco),
      sorter: (a, b) => a.preco - b.preco,
      onCell: (record) => ({
        record,
        dataIndex: 'preco',
        title: t('partsCatalog.price'),
        editing: isEditing(record),
      }),
    },
    {
      title: t('partsCatalog.stock'),
      dataIndex: 'estoque',
      key: 'estoque',
      render: getEstoqueTag,
      sorter: (a, b) => a.estoque - b.estoque,
      onCell: (record) => ({
        record,
        dataIndex: 'estoque',
        title: t('partsCatalog.stock'),
        editing: isEditing(record),
      }),
    },
    {
      title: t('partsCatalog.status'),
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
            checkedChildren={t('partsCatalog.status.active')}
            disabled={editingKey !== null || savingKey === record.id}
            loading={savingKey === record.id}
            onChange={() => toggleStatus(record)}
            unCheckedChildren={t('partsCatalog.status.inactive')}
          />
        ),
    },
    {
      title: t('partsCatalog.actions'),
      key: 'actions',
      width: 1,
      render: (_: unknown, record: PecaResponse) => {
        const editable = isEditing(record);
        return editable ? (
          <Space>
            <Button type="link" loading={savingKey === record.id} onClick={() => save(record)}>
              {t('partsCatalog.save')}
            </Button>
            <Button type="link" danger onClick={cancel}>
              {t('partsCatalog.cancel')}
            </Button>
          </Space>
        ) : (
          <Button
            type="link"
            icon={<EditOutlined />}
            disabled={editingKey !== null}
            onClick={() => edit(record)}
          >
            {t('partsCatalog.edit')}
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
                  <span>{t('dashboard.menu.catalogos')}</span>
                </>
              ),
            },
            {
              title: t('partsCatalog.title'),
            },
          ]}
        />

        <Title level={2} style={{ margin: 0 }}>
          {t('partsCatalog.title')}
        </Title>

        <Flex gap="middle" align="center" wrap="wrap">
          <Input
            allowClear
            placeholder={t('partsCatalog.searchPlaceholder')}
            prefix={<SearchOutlined />}
            style={{ width: 300 }}
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />

          <Select
            allowClear
            options={getCategoriaPecaOptions()}
            placeholder={t('partsCatalog.category')}
            style={{ width: 160 }}
            value={categoria}
            onChange={setCategoria}
          />

          <Select
            allowClear
            options={[
              { value: 'disponivel', label: t('partsCatalog.stock.available') },
              { value: 'baixo', label: t('partsCatalog.stock.low') },
              { value: 'zerado', label: t('partsCatalog.stock.empty') },
            ]}
            placeholder={t('partsCatalog.stock')}
            style={{ width: 160 }}
            value={estoque}
            onChange={setEstoque}
          />

          <Select
            options={[
              { value: 'Ativo', label: t('partsCatalog.status.activePlural') },
              { value: 'Inativo', label: t('partsCatalog.status.inactivePlural') },
              { value: 'Todos', label: t('partsCatalog.status.all') },
            ]}
            placeholder={t('partsCatalog.status')}
            style={{ width: 140 }}
            value={status}
            onChange={setStatus}
          />

          <Flex gap="small" style={{ marginLeft: 'auto' }}>
            <Button onClick={clearFilters}>{t('partsCatalog.clear')}</Button>
            <Button icon={<ReloadOutlined />} loading={loading} onClick={loadPecas}>
              {t('partsCatalog.refresh')}
            </Button>
          </Flex>
        </Flex>
      </Flex>

      {error && <Alert type="error" message={error} showIcon />}

      <Card>
        <Flex vertical gap="middle" style={{ width: '100%' }}>
          <Flex justify="space-between" align="center" wrap="wrap" gap="middle">
            <Text>{t('partsCatalog.totalParts', { count: filteredPecas.length })}</Text>
            <Button
              type="primary"
              disabled={editingKey !== null}
              onClick={() => navigate(PATHS.CONCESSIONARIA_CATALOGOS_PECAS_CREATE)}
            >
              {t('partsCatalog.addPart')}
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
