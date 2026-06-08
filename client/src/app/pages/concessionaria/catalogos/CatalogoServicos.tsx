import { useEffect, useMemo, useState } from 'react';
import {
  Button,
  Empty,
  Flex,
  Form,
  Input,
  InputNumber,
  message,
  Modal,
  Select,
  Space,
  Spin,
  Switch,
  Table,
  Tag,
  Typography,
} from 'antd';
import { EditOutlined, ReloadOutlined, SearchOutlined, ToolOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { getLocale, t } from '@/app/i18n';
import { Servico } from '@/app/models/Servico';
import { ServicoRequest } from '@/app/models/ServicoRequest';
import { PATH_SEGMENTS } from '@/app/paths';
import { servicoService } from '@/app/services/servicoService';
import { handleApiError } from '@/app/utils/errorHandler';

const { Title } = Typography;

type CategoriaServico = 'Verificacao' | 'Ajuste' | 'Limpeza' | 'Troca';
type TempoFilter = 'ate30' | '31a60' | 'mais60';
type PrecoFilter = 'ate100' | '101a200' | 'mais200';

interface CatalogoServicosProps {
  onNavigateToForm?: () => void;
}

interface ServicoFormValues {
  codigo: string;
  nome: string;
  categoria: CategoriaServico;
  tempoEstimado: number;
  custo: number;
  descricao: string;
}

interface EditableCellProps {
  editing: boolean;
  dataIndex: keyof ServicoFormValues;
  title: string;
  children: React.ReactNode;
}

const categoriaOptions: Array<{ value: CategoriaServico; labelKey: string }> = [
  { value: 'Verificacao', labelKey: 'serviceCatalog.category.verificacao' },
  { value: 'Ajuste', labelKey: 'serviceCatalog.category.ajuste' },
  { value: 'Limpeza', labelKey: 'serviceCatalog.category.limpeza' },
  { value: 'Troca', labelKey: 'serviceCatalog.category.troca' },
];

function getCategoriaOptions() {
  return categoriaOptions.map(({ value, labelKey }) => ({ value, label: t(labelKey) }));
}

function translateCategoria(categoria: string) {
  const option = categoriaOptions.find((currentOption) => currentOption.value === categoria);
  return option ? t(option.labelKey) : categoria;
}

function formatCurrency(value: number) {
  return value.toLocaleString(getLocale(), {
    style: 'currency',
    currency: 'BRL',
  });
}

function formatTempoEstimado(tempo: number) {
  if (tempo < 60) return `${tempo} min`;

  const horas = Math.floor(tempo / 60);
  const minutos = tempo % 60;

  return minutos > 0 ? `${horas}h ${minutos} min` : `${horas}h`;
}

function getDecimalSeparator() {
  return getLocale() === 'pt-BR' ? ',' : '.';
}

function toUpdateRequest(values: ServicoFormValues): ServicoRequest {
  return {
    codigo: values.codigo,
    nome: values.nome,
    categoria: values.categoria,
    tempoEstimado: Number(values.tempoEstimado),
    custo: Number(values.custo),
    descricao: values.descricao,
  };
}

const EditableCell: React.FC<EditableCellProps> = ({
  editing,
  dataIndex,
  title,
  children,
  ...restProps
}) => {
  let inputNode: React.ReactNode;

  if (dataIndex === 'categoria') {
    inputNode = <Select options={getCategoriaOptions()} />;
  } else if (dataIndex === 'tempoEstimado') {
    inputNode = <InputNumber min={1} max={480} precision={0} style={{ width: '100%' }} />;
  } else if (dataIndex === 'custo') {
    inputNode = (
      <InputNumber
        addonBefore="R$"
        decimalSeparator={getDecimalSeparator()}
        min={0}
        precision={2}
        step={0.01}
        style={{ width: '100%' }}
      />
    );
  } else if (dataIndex === 'descricao') {
    inputNode = <Input.TextArea rows={2} maxLength={500} />;
  } else {
    inputNode = <Input />;
  }

  return (
    <td {...restProps}>
      {editing ? (
        <Form.Item
          name={dataIndex}
          style={{ margin: 0 }}
          rules={[{ required: true, message: t('serviceCatalog.enterField', { field: title }) }]}
        >
          {inputNode}
        </Form.Item>
      ) : (
        children
      )}
    </td>
  );
};

export default function CatalogoServicos({ onNavigateToForm }: CatalogoServicosProps) {
  const [form] = Form.useForm<ServicoFormValues>();
  const [servicos, setServicos] = useState<Servico[]>([]);
  const [loading, setLoading] = useState(false);
  const [savingKey, setSavingKey] = useState<number | null>(null);
  const [editingKey, setEditingKey] = useState<number | null>(null);
  const [searchText, setSearchText] = useState('');
  const [categoryFilter, setCategoryFilter] = useState<CategoriaServico>();
  const [tempoFilter, setTempoFilter] = useState<TempoFilter>();
  const [precoFilter, setPrecoFilter] = useState<PrecoFilter>();

  const loadServicos = async () => {
    setLoading(true);

    try {
      const data = await servicoService.getCatalogo();
      setServicos(data);
    } catch (error) {
      handleApiError(error, 'error.fetchServices');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadServicos();
  }, []);

  const filteredServicos = useMemo(() => {
    const normalizedSearch = searchText.trim().toLowerCase();

    return servicos.filter((servico) => {
      const matchesSearch =
        !normalizedSearch ||
        servico.codigo.toLowerCase().includes(normalizedSearch) ||
        servico.nome.toLowerCase().includes(normalizedSearch) ||
        servico.descricao.toLowerCase().includes(normalizedSearch);

      const matchesCategoria = !categoryFilter || servico.categoria === categoryFilter;

      const matchesTempo =
        !tempoFilter ||
        (tempoFilter === 'ate30' && servico.tempoEstimado <= 30) ||
        (tempoFilter === '31a60' && servico.tempoEstimado > 30 && servico.tempoEstimado <= 60) ||
        (tempoFilter === 'mais60' && servico.tempoEstimado > 60);

      const matchesPreco =
        !precoFilter ||
        (precoFilter === 'ate100' && servico.custo <= 100) ||
        (precoFilter === '101a200' && servico.custo > 100 && servico.custo <= 200) ||
        (precoFilter === 'mais200' && servico.custo > 200);

      return matchesSearch && matchesCategoria && matchesTempo && matchesPreco;
    });
  }, [servicos, searchText, categoryFilter, tempoFilter, precoFilter]);

  const isEditing = (record: Servico) => record.id === editingKey;

  const edit = (record: Servico) => {
    form.setFieldsValue({
      codigo: record.codigo,
      nome: record.nome,
      categoria: record.categoria as CategoriaServico,
      tempoEstimado: record.tempoEstimado,
      custo: record.custo,
      descricao: record.descricao,
    });
    setEditingKey(record.id);
  };

  const cancel = () => {
    Modal.confirm({
      title: t('serviceCatalog.cancelEdit.title'),
      content: t('serviceCatalog.cancelEdit.content'),
      okText: t('yes'),
      cancelText: t('no'),
      onOk() {
        form.resetFields();
        setEditingKey(null);
      },
    });
  };

  const save = async (record: Servico) => {
    try {
      const values = await form.validateFields();

      Modal.confirm({
        title: t('serviceCatalog.saveEdit.title'),
        content: t('serviceCatalog.saveEdit.content'),
        okText: t('yes'),
        cancelText: t('no'),
        async onOk() {
          setSavingKey(record.id);

          try {
            const updatedServico = await servicoService.update(record.id, toUpdateRequest(values));
            setServicos((currentServicos) =>
              currentServicos.map((servico) => (servico.id === record.id ? updatedServico : servico)),
            );
            form.resetFields();
            setEditingKey(null);
            message.success(t('serviceUpdatedSuccess'));
          } catch (error) {
            handleApiError(error, 'error.updateService');
          } finally {
            setSavingKey(null);
          }
        },
      });
    } catch {
      message.error(t('serviceCatalog.save.validationError'));
    }
  };

  const toggleStatus = async (record: Servico) => {
    const nextAtivo = !record.ativo;

    const updateStatus = async () => {
      setSavingKey(record.id);

      try {
        const updatedServico = await servicoService.alternarStatus(record.id);
        setServicos((currentServicos) =>
          currentServicos.map((servico) => (servico.id === record.id ? updatedServico : servico)),
        );
        message.success(
          updatedServico.ativo
            ? t('serviceCatalog.activate.success')
            : t('serviceCatalog.deactivate.success'),
        );
      } catch (error) {
        handleApiError(error, 'serviceCatalog.status.error');
      } finally {
        setSavingKey(null);
      }
    };

    if (!nextAtivo) {
      Modal.confirm({
        title: t('serviceCatalog.deactivate.title'),
        content: t('serviceCatalog.deactivate.content'),
        okText: t('yes'),
        cancelText: t('no'),
        onOk: updateStatus,
      });
      return;
    }

    await updateStatus();
  };

  const handleClearFilters = () => {
    setSearchText('');
    setCategoryFilter(undefined);
    setTempoFilter(undefined);
    setPrecoFilter(undefined);
    loadServicos();
  };

  const emptyText =
    servicos.length === 0
      ? t('serviceCatalog.empty')
      : t('serviceCatalog.emptyFiltered');

  const columns: ColumnsType<Servico> = [
    {
      title: t('serviceCatalog.code'),
      dataIndex: 'codigo',
      key: 'codigo',
      width: 130,
      sorter: (a, b) => a.codigo.localeCompare(b.codigo),
      onCell: (record) => ({
        record,
        dataIndex: 'codigo',
        title: t('serviceCatalog.code'),
        editing: isEditing(record),
      }),
    },
    {
      title: t('serviceCatalog.serviceName'),
      dataIndex: 'nome',
      key: 'nome',
      width: 220,
      sorter: (a, b) => a.nome.localeCompare(b.nome),
      onCell: (record) => ({
        record,
        dataIndex: 'nome',
        title: t('serviceCatalog.serviceName'),
        editing: isEditing(record),
      }),
    },
    {
      title: t('serviceCatalog.category'),
      dataIndex: 'categoria',
      key: 'categoria',
      width: 160,
      render: translateCategoria,
      onCell: (record) => ({
        record,
        dataIndex: 'categoria',
        title: t('serviceCatalog.category'),
        editing: isEditing(record),
      }),
    },
    {
      title: t('serviceCatalog.estimatedTime'),
      dataIndex: 'tempoEstimado',
      key: 'tempoEstimado',
      width: 170,
      render: formatTempoEstimado,
      sorter: (a, b) => a.tempoEstimado - b.tempoEstimado,
      onCell: (record) => ({
        record,
        dataIndex: 'tempoEstimado',
        title: t('serviceCatalog.estimatedTime'),
        editing: isEditing(record),
      }),
    },
    {
      title: t('serviceCatalog.price'),
      dataIndex: 'custo',
      key: 'custo',
      width: 150,
      render: formatCurrency,
      sorter: (a, b) => a.custo - b.custo,
      onCell: (record) => ({
        record,
        dataIndex: 'custo',
        title: t('serviceCatalog.price'),
        editing: isEditing(record),
      }),
    },
    {
      title: t('serviceCatalog.description'),
      dataIndex: 'descricao',
      key: 'descricao',
      ellipsis: true,
      onCell: (record) => ({
        record,
        dataIndex: 'descricao',
        title: t('serviceCatalog.description'),
        editing: isEditing(record),
      }),
    },
    {
      title: t('serviceCatalog.status'),
      dataIndex: 'ativo',
      key: 'ativo',
      width: 160,
      align: 'center',
      render: (_ativo: boolean, record) =>
        isEditing(record) ? (
          <Tag color={record.ativo ? 'green' : 'red'}>
            {record.ativo ? t('status.activeSingle') : t('status.inactiveSingle')}
          </Tag>
        ) : (
          <Switch
            checked={record.ativo}
            checkedChildren={t('status.activeSingle')}
            disabled={editingKey !== null || savingKey === record.id}
            loading={savingKey === record.id}
            onChange={() => toggleStatus(record)}
            unCheckedChildren={t('status.inactiveSingle')}
          />
        ),
    },
    {
      title: t('serviceCatalog.actions'),
      key: 'actions',
      width: 150,
      render: (_: unknown, record) => {
        const editable = isEditing(record);
        return editable ? (
          <Space>
            <Button type="link" loading={savingKey === record.id} onClick={() => save(record)}>
              {t('serviceCatalog.save')}
            </Button>
            <Button type="link" danger onClick={cancel}>
              {t('serviceCatalog.cancel')}
            </Button>
          </Space>
        ) : (
          <Button
            type="link"
            icon={<EditOutlined />}
            disabled={editingKey !== null}
            onClick={() => edit(record)}
          >
            {t('serviceCatalog.edit')}
          </Button>
        );
      },
    },
  ];

  return (
    <Flex vertical gap="large" style={{ width: '100%' }}>
      <Flex vertical gap="middle" style={{ width: '100%' }}>
        <DashboardBreadcrumb
          userType="concessionaria"
          items={[
            {
              title: t('dashboard.menu.catalogos'),
              icon: <ToolOutlined />,
            },
            {
              title: t('serviceCatalog.title'),
            },
          ]}
        />

        <Title level={2} style={{ margin: 0 }}>
          {t('serviceCatalog.title')}
        </Title>

        <Flex gap="middle" align="center" wrap="wrap">
          <Input
            allowClear
            placeholder={t('serviceCatalog.searchPlaceholder')}
            prefix={<SearchOutlined />}
            style={{ width: 300 }}
            value={searchText}
            onChange={(event) => setSearchText(event.target.value)}
          />

          <Select
            allowClear
            options={getCategoriaOptions()}
            placeholder={t('serviceCatalog.category')}
            style={{ width: 150 }}
            value={categoryFilter}
            onChange={setCategoryFilter}
          />

          <Select
            allowClear
            options={[
              { value: 'ate30', label: t('serviceCatalog.time.upTo30') },
              { value: '31a60', label: t('serviceCatalog.time.from31To60') },
              { value: 'mais60', label: t('serviceCatalog.time.moreThan60') },
            ]}
            placeholder={t('serviceCatalog.time.placeholder')}
            style={{ width: 150 }}
            value={tempoFilter}
            onChange={setTempoFilter}
          />

          <Select
            allowClear
            options={[
              { value: 'ate100', label: t('serviceCatalog.price.upTo100') },
              { value: '101a200', label: t('serviceCatalog.price.from101To200') },
              { value: 'mais200', label: t('serviceCatalog.price.moreThan200') },
            ]}
            placeholder={t('serviceCatalog.price')}
            style={{ width: 150 }}
            value={precoFilter}
            onChange={setPrecoFilter}
          />

          <Flex gap="small" style={{ marginLeft: 'auto' }}>
            <Button onClick={handleClearFilters}>{t('serviceCatalog.clear')}</Button>
            <Button icon={<ReloadOutlined />} loading={loading} onClick={loadServicos}>
              {t('serviceCatalog.refresh')}
            </Button>
          </Flex>
        </Flex>
      </Flex>

      <div style={{ background: '#fff', padding: '24px', borderRadius: '8px' }}>
        <Spin spinning={loading}>
          <Space direction="vertical" size="middle" style={{ width: '100%' }}>
            <Flex justify="space-between" align="center">
              <span>{t('serviceCatalog.totalServices', { count: filteredServicos.length })}</span>
              <Button type="primary" onClick={onNavigateToForm}>
                {t('serviceCatalog.addService')}
              </Button>
            </Flex>

            <Form form={form} component={false}>
              <Table
                components={{
                  body: {
                    cell: EditableCell,
                  },
                }}
                columns={columns}
                dataSource={filteredServicos}
                locale={{
                  emptyText: (
                    <Empty
                      image={Empty.PRESENTED_IMAGE_SIMPLE}
                      description={emptyText}
                    />
                  ),
                }}
                pagination={{
                  onChange: () => setEditingKey(null),
                  pageSize: 10,
                }}
                rowKey="id"
              />
            </Form>
          </Space>
        </Spin>
      </div>
    </Flex>
  );
}
