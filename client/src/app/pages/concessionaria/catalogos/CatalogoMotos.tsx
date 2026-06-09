import { useEffect, useMemo, useState } from 'react';
import { Typography, Input, InputNumber, Select, Button, Table, Space, Flex, Form, Popconfirm, message, Spin, Tag, Card, Empty, Switch, AutoComplete } from 'antd';
import { CarOutlined, SearchOutlined, EditOutlined, SaveOutlined, CloseOutlined, ReloadOutlined } from '@ant-design/icons';
import type { ColumnType } from 'antd/es/table';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { modeloMotoService } from '@/app/services/modeloMotoService';
import { linhaService } from '@/app/services/linhaService';
import { ModeloMoto } from '@/app/models/ModeloMoto';
import { Linha } from '@/app/models/Linha';
import { ModeloMotoRequest } from '@/app/models/ModeloMotoRequest';
import { handleApiError } from '@/app/utils/errorHandler';
import { getLocale, t } from '@/app/i18n';

const { Title } = Typography;

interface ModeloMotoData extends ModeloMoto {
  key: string;
}

interface CatalogoMotosProps {
  onNavigateToForm?: () => void;
}

interface EditableColumn extends ColumnType<ModeloMotoData> {
  editable?: boolean;
  required?: boolean;
  rules?: any[];
}

interface EditableCellProps {
  editing: boolean;
  dataIndex: string;
  cellTitle: React.ReactNode;
  required?: boolean;
  rules?: any[];
  children: React.ReactNode;
  linhas: Linha[];
  marcaOptions: SelectOption[];
}

interface SelectOption {
  value: string;
  label: string;
}

type StatusFilter = 'active' | 'inactive' | 'all';

const currentYear = new Date().getFullYear();
const minModelYear = 1901;

const EditableCell: React.FC<EditableCellProps> = ({
  editing,
  dataIndex,
  cellTitle,
  required = true,
  rules,
  children,
  linhas = [],
  marcaOptions = [],
  ...restProps
}) => {
  const inputNodeMap: Record<string, React.ReactNode> = {
    marca: (
      <AutoComplete
        options={marcaOptions}
        filterOption={(inputValue, option) =>
          String(option?.value ?? '').toLowerCase().includes(inputValue.toLowerCase())
        }
      />
    ),
    linhaId: <Select options={linhas.map((linha) => ({ value: linha.id, label: linha.nome }))} />,
    ano: (
      <InputNumber
        min={0}
        precision={0}
        parser={(value) => value?.replace(/[^\d]/g, '') as any}
      />
    ),
    cilindrada: <Input placeholder={t('modeloMotoCatalog.engineShortPlaceholder')} />,
  };

  const inputNode = inputNodeMap[dataIndex] || <Input />;
  const { record: _record, index: _index, ...tdProps } = restProps as any;

  return (
    <td {...tdProps}>
      {editing ? (
        <Form.Item
          name={dataIndex}
          style={{ margin: 0 }}
          rules={rules || [
            {
              required,
              message: t('modeloMotoCatalog.enterField', { field: String(cellTitle) }),
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

export default function CatalogoMotos({ onNavigateToForm }: CatalogoMotosProps) {
  const [form] = Form.useForm();
  const [data, setData] = useState<ModeloMotoData[]>([]);
  const [linhas, setLinhas] = useState<Linha[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingKey, setEditingKey] = useState('');
  const [searchText, setSearchText] = useState('');
  const [marcaFilter, setMarcaFilter] = useState<string | undefined>();
  const [linhaFilter, setLinhaFilter] = useState<number | undefined>();
  const [anoFilter, setAnoFilter] = useState<number | undefined>();
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('active');

  const linhaById = useMemo(() => {
    return new Map(linhas.map((linha) => [linha.id, linha.nome]));
  }, [linhas]);

  const marcaOptions = useMemo<SelectOption[]>(() => {
    const marcas = data
      .map((modelo) => modelo.marca.trim())
      .filter(Boolean);

    return Array.from(new Set(marcas))
      .sort((a, b) => a.localeCompare(b, getLocale()))
      .map((marca) => ({ value: marca, label: marca }));
  }, [data]);

  const fetchModelos = async () => {
    try {
      setLoading(true);
      const [modelos, linhasData] = await Promise.all([
        modeloMotoService.getCatalogo(),
        linhaService.getAll(false),
      ]);

      setData(modelos.map((modelo) => ({ ...modelo, key: modelo.id.toString() })));
      setLinhas(linhasData);
    } catch (error) {
      handleApiError(error, 'error.fetchModelosMotos');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchModelos();
  }, []);

  const filteredData = useMemo(() => {
    const lowerSearch = searchText.toLowerCase();

    return data.filter((item) => {
      const linhaNome = linhaById.get(item.linhaId) || item.linhaId.toString();
      const matchesSearch = !searchText
        || item.nomeModelo.toLowerCase().includes(lowerSearch)
        || item.marca.toLowerCase().includes(lowerSearch)
        || linhaNome.toLowerCase().includes(lowerSearch);
      const matchesMarca = !marcaFilter || item.marca === marcaFilter;
      const matchesLinha = !linhaFilter || item.linhaId === linhaFilter;
      const matchesAno = !anoFilter || item.ano === anoFilter;
      const matchesStatus = statusFilter === 'all'
        || (statusFilter === 'active' && item.ativo)
        || (statusFilter === 'inactive' && !item.ativo);

      return matchesSearch && matchesMarca && matchesLinha && matchesAno && matchesStatus;
    });
  }, [anoFilter, data, linhaById, linhaFilter, marcaFilter, searchText, statusFilter]);

  const isEditing = (record: ModeloMotoData) => record.key === editingKey;

  const edit = (record: ModeloMotoData) => {
    form.setFieldsValue({ ...record });
    setEditingKey(record.key);
  };

  const cancel = () => {
    setEditingKey('');
  };

  const buildRequest = (record: ModeloMotoData, values: Partial<ModeloMotoRequest>): ModeloMotoRequest => ({
    nomeModelo: (values.nomeModelo ?? record.nomeModelo).trim(),
    marca: values.marca ?? record.marca,
    linhaId: values.linhaId ?? record.linhaId,
    cilindrada: values.cilindrada ?? record.cilindrada,
    ano: values.ano ?? record.ano,
  });

  const save = async (key: string) => {
    try {
      const row = await form.validateFields();
      const record = data.find((item) => item.key === key);
      if (!record) return;

      setLoading(true);
      await modeloMotoService.update(record.id, buildRequest(record, row));
      setEditingKey('');
      await fetchModelos();
      message.success(t('modeloMotoUpdatedSuccess'));
    } catch (error) {
      handleApiError(error, 'error.updateModeloMoto', { conflictKey: 'error.modeloMotoConflict' });
    } finally {
      setLoading(false);
    }
  };

  const handleStatusToggle = async (record: ModeloMotoData) => {
    try {
      setLoading(true);
      await modeloMotoService.alternarStatus(record.id);
      await fetchModelos();
      message.success(t('modeloMotoStatusUpdatedSuccess'));
    } catch (error) {
      handleApiError(error, 'error.toggleModeloMotoStatus', { conflictKey: 'error.modeloMotoConflict' });
    } finally {
      setLoading(false);
    }
  };

  const handleClearFilters = () => {
    setSearchText('');
    setMarcaFilter(undefined);
    setLinhaFilter(undefined);
    setAnoFilter(undefined);
    setStatusFilter('active');
  };

  const columns: EditableColumn[] = [
    {
      title: t('modeloMotoCatalog.brand'),
      dataIndex: 'marca',
      key: 'marca',
      editable: true,
    },
    {
      title: t('modeloMotoCatalog.model'),
      dataIndex: 'nomeModelo',
      key: 'nomeModelo',
      editable: true,
    },
    {
      title: t('modeloMotoCatalog.year'),
      dataIndex: 'ano',
      key: 'ano',
      editable: true,
      required: false,
      rules: [
        {
          type: 'number',
          min: minModelYear,
          max: currentYear,
          message: t('modeloMotoCatalog.yearRange', { min: minModelYear, max: currentYear }),
        },
      ],
      render: (ano?: number) => ano ?? '-',
    },
    {
      title: t('modeloMotoCatalog.engine'),
      dataIndex: 'cilindrada',
      key: 'cilindrada',
      editable: true,
      required: false,
      render: (cilindrada?: string) => cilindrada || '-',
    },
    {
      title: t('modeloMotoCatalog.line'),
      dataIndex: 'linhaId',
      key: 'linhaId',
      editable: true,
      render: (linhaId: number) => linhaById.get(linhaId) || linhaId,
    },
    {
      title: t('modeloMotoCatalog.status'),
      key: 'status',
      width: 120,
      align: 'center',
      render: (_: any, record: ModeloMotoData) => {
        const editable = isEditing(record);
        return editable ? (
          record.ativo ? <Tag color="green">{t('status.activeSingle')}</Tag> : <Tag color="red">{t('status.inactiveSingle')}</Tag>
        ) : (
          <Popconfirm
            title={record.ativo ? t('modeloMotoCatalog.deactivateTitle') : t('modeloMotoCatalog.activateTitle')}
            description={record.ativo ? t('modeloMotoCatalog.deactivateDescription') : t('modeloMotoCatalog.activateDescription')}
            onConfirm={() => handleStatusToggle(record)}
            okText={t('yes')}
            cancelText={t('no')}
            disabled={editingKey !== ''}
          >
            <Switch
              checked={record.ativo}
              checkedChildren={t('status.activeSingle')}
              unCheckedChildren={t('status.inactiveSingle')}
              disabled={editingKey !== ''}
            />
          </Popconfirm>
        );
      },
    },
    {
      title: t('modeloMotoCatalog.actions'),
      key: 'actions',
      width: 160,
      render: (_: any, record: ModeloMotoData) => {
        const editable = isEditing(record);
        return editable ? (
          <Space size="small">
            <Popconfirm
              title={t('modeloMotoCatalog.saveTitle')}
              description={t('modeloMotoCatalog.saveDescription')}
              onConfirm={() => save(record.key)}
              okText={t('yes')}
              cancelText={t('no')}
            >
              <Button type="link" icon={<SaveOutlined />}>
                {t('modeloMotoCatalog.save')}
              </Button>
            </Popconfirm>
            <Button type="link" danger icon={<CloseOutlined />} onClick={cancel}>
              {t('modeloMotoCatalog.cancel')}
            </Button>
          </Space>
        ) : (
          <Button
            type="link"
            icon={<EditOutlined />}
            disabled={editingKey !== ''}
            onClick={() => edit(record)}
          >
            {t('modeloMotoCatalog.edit')}
          </Button>
        );
      },
    },
  ];

  const mergedColumns = columns.map((col) => {
    if (!col.editable) {
      return col;
    }
    const { editable, required, ...colWithoutEditable } = col;
    return {
      ...colWithoutEditable,
      onCell: (record: ModeloMotoData) => ({
        record,
        dataIndex: col.dataIndex,
        cellTitle: col.title,
        required,
        rules: col.rules,
        editing: isEditing(record),
        linhas,
        marcaOptions,
      }),
    };
  });

  return (
    <Spin spinning={loading}>
      <Space orientation="vertical" size="large" style={{ width: '100%' }}>
        <Space orientation="vertical" size="middle" style={{ width: '100%' }}>
          <DashboardBreadcrumb
            userType="concessionaria"
            items={[
              {
                title: t('modeloMotoCatalog.title'),
                icon: <CarOutlined />,
              },
            ]}
          />

          <Title level={2} style={{ margin: 0 }}>
            {t('modeloMotoCatalog.title')}
          </Title>

          <Flex gap="middle" align="center" wrap="wrap">
            <Input
              placeholder={t('modeloMotoCatalog.searchPlaceholder')}
              prefix={<SearchOutlined />}
              style={{ width: 300 }}
              value={searchText}
              onChange={(event) => setSearchText(event.target.value)}
              allowClear
            />

            <Select
              placeholder={t('modeloMotoCatalog.brand')}
              style={{ width: 150 }}
              value={marcaFilter}
              onChange={setMarcaFilter}
              allowClear
              options={marcaOptions}
            />

            <Select
              placeholder={t('modeloMotoCatalog.line')}
              style={{ width: 150 }}
              value={linhaFilter}
              onChange={setLinhaFilter}
              allowClear
              options={linhas.map((linha) => ({ value: linha.id, label: linha.nome }))}
            />

            <InputNumber
              placeholder={t('modeloMotoCatalog.year')}
              style={{ width: 120 }}
              value={anoFilter}
              onChange={(value) => setAnoFilter(typeof value === 'number' ? value : undefined)}
              min={minModelYear}
              max={currentYear}
              precision={0}
              parser={(value) => value?.replace(/[^\d]/g, '') as any}
            />

            <Select
              placeholder={t('status.placeholder')}
              style={{ width: 140 }}
              value={statusFilter}
              onChange={setStatusFilter}
              options={[
                { value: 'active', label: t('status.active') },
                { value: 'inactive', label: t('status.inactive') },
                { value: 'all', label: t('status.all') },
              ]}
            />

            <Flex gap="small" style={{ marginLeft: 'auto' }}>
              <Button onClick={handleClearFilters}>{t('modeloMotoCatalog.clear')}</Button>
              <Button type="primary" icon={<ReloadOutlined />} onClick={fetchModelos}>
                {t('modeloMotoCatalog.refresh')}
              </Button>
            </Flex>
          </Flex>
        </Space>

        <Card>
          <Space orientation="vertical" size="middle" style={{ width: '100%' }}>
            <Flex justify="space-between" align="center">
              <span>{t('modeloMotoCatalog.total', { count: filteredData.length })}</span>
              <Button type="primary" onClick={onNavigateToForm}>
                {t('modeloMotoCatalog.add')}
              </Button>
            </Flex>

            <Form form={form} component={false}>
              <Table
                components={{
                  body: {
                    cell: EditableCell,
                  },
                }}
                dataSource={filteredData}
                columns={mergedColumns as ColumnType<ModeloMotoData>[]}
                pagination={{
                  onChange: cancel,
                }}
                locale={{
                  emptyText: (
                    <Empty
                      image={Empty.PRESENTED_IMAGE_SIMPLE}
                      description={t('modeloMotoCatalog.empty')}
                    />
                  ),
                }}
              />
            </Form>
          </Space>
        </Card>
      </Space>
    </Spin>
  );
}
