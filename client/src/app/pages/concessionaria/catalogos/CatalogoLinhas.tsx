import React, { useEffect, useMemo, useState } from 'react';
import {
  Button,
  Empty,
  Flex,
  Form,
  Input,
  message,
  Modal,
  Select,
  Space,
  Spin,
  Switch,
  Table,
  Tag,
  theme,
  Typography,
} from 'antd';
import { EditOutlined, ReloadOutlined, SearchOutlined, ToolOutlined } from '@ant-design/icons';
import type { ColumnType } from 'antd/es/table';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { Linha } from '@/app/models/Linha';
import { ModeloMoto } from '@/app/models/ModeloMoto';
import { PATH_SEGMENTS } from '@/app/paths';
import { linhaService } from '@/app/services/linhaService';
import { modeloMotoService } from '@/app/services/modeloMotoService';
import { handleApiError } from '@/app/utils/errorHandler';
import { t } from '@/app/i18n';

const { Title } = Typography;

type StatusFilter = 'active' | 'inactive' | 'all';

interface LinhaData extends Linha {
  key: string;
}

interface CatalogoLinhasProps {
  onNavigateToForm?: () => void;
}

interface LinhaFormValues {
  nome: string;
  descricao?: string;
}

interface EditableColumn extends ColumnType<LinhaData> {
  editable?: boolean;
  dataIndex?: keyof LinhaFormValues;
}

interface EditableCellProps {
  editing: boolean;
  dataIndex: keyof LinhaFormValues;
  title: string;
  children: React.ReactNode;
}

const EditableCell: React.FC<EditableCellProps> = ({
  editing,
  dataIndex,
  title,
  children,
  ...restProps
}) => {
  const inputNode = dataIndex === 'descricao' ? <Input.TextArea rows={2} /> : <Input />;

  return (
    <td {...restProps}>
      {editing ? (
        <Form.Item
          name={dataIndex}
          style={{ margin: 0 }}
          rules={[
            {
              required: true,
              message: t('lineCatalog.requiredField', { field: title }),
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

export default function CatalogoLinhas({ onNavigateToForm }: CatalogoLinhasProps) {
  const [form] = Form.useForm<LinhaFormValues>();
  const { token } = theme.useToken();
  const [data, setData] = useState<LinhaData[]>([]);
  const [modelos, setModelos] = useState<ModeloMoto[]>([]);
  const [loading, setLoading] = useState(true);
  const [savingKey, setSavingKey] = useState<string | null>(null);
  const [editingKey, setEditingKey] = useState('');
  const [searchText, setSearchText] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('active');

  const fetchLinhas = async () => {
    try {
      setLoading(true);
      const apenasAtivos = statusFilter === 'active';
      const [linhas, models] = await Promise.all([
        linhaService.getAll(apenasAtivos),
        modeloMotoService.getAll(),
      ]);
      const filteredLinhas = statusFilter === 'inactive'
        ? linhas.filter((linha) => !linha.ativo)
        : linhas;

      setData(filteredLinhas.map((linha) => ({ ...linha, key: linha.id.toString() })));
      setModelos(models);
    } catch (error) {
      handleApiError(error, 'error.fetchLinhas');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchLinhas();
  }, [statusFilter]);

  const filteredData = useMemo(() => {
    const lowerSearch = searchText.trim().toLowerCase();

    if (!lowerSearch) return data;

    return data.filter((item) =>
      item.nome.toLowerCase().includes(lowerSearch)
      || (item.descricao ?? '').toLowerCase().includes(lowerSearch),
    );
  }, [data, searchText]);

  const isEditing = (record: LinhaData) => record.key === editingKey;

  const countModelosVinculados = (linhaId: number) =>
    modelos.filter((modelo) => modelo.linhaId === linhaId).length;

  const edit = (record: LinhaData) => {
    form.setFieldsValue({
      nome: record.nome,
      descricao: record.descricao,
    });
    setEditingKey(record.key);
  };

  const cancel = () => {
    Modal.confirm({
      title: t('lineCatalog.cancelEdit.title'),
      content: t('lineCatalog.cancelEdit.content'),
      okText: t('yes'),
      cancelText: t('no'),
      onOk() {
        form.resetFields();
        setEditingKey('');
      },
    });
  };

  const save = async (record: LinhaData) => {
    try {
      const row = await form.validateFields();

      Modal.confirm({
        title: t('lineCatalog.saveEdit.title'),
        content: t('lineCatalog.saveEdit.content'),
        okText: t('yes'),
        cancelText: t('no'),
        async onOk() {
          setSavingKey(record.key);

          try {
            await linhaService.update(record.id, {
              nome: row.nome,
              descricao: row.descricao,
            });
            form.resetFields();
            setEditingKey('');
            await fetchLinhas();
            message.success(t('linhaUpdatedSuccess'));
          } catch (error) {
            handleApiError(error, 'error.updateLinha');
          } finally {
            setSavingKey(null);
          }
        },
      });
    } catch {
      message.error(t('lineCatalog.save.validationError'));
    }
  };

  const toggleStatus = async (record: LinhaData) => {
    if (record.ativo) {
      const temModelosAtivos = modelos.some((modelo) => modelo.linhaId === record.id && modelo.ativo);
      if (temModelosAtivos) {
        message.error(t('lineCatalog.deactivate.blocked'));
        return;
      }

      Modal.confirm({
        title: t('lineCatalog.deactivate.title'),
        content: t('lineCatalog.deactivate.content'),
        okText: t('yes'),
        cancelText: t('no'),
        onOk: () => updateStatus(record),
      });
      return;
    }

    await updateStatus(record);
  };

  const updateStatus = async (record: LinhaData) => {
    try {
      setSavingKey(record.key);
      await linhaService.alternarStatus(record.id);
      await fetchLinhas();
      message.success(t('linhaStatusUpdatedSuccess'));
    } catch (error) {
      handleApiError(error, 'error.toggleLinhaStatus');
    } finally {
      setSavingKey(null);
    }
  };

  const handleClearFilters = () => {
    setSearchText('');
    setStatusFilter('active');
  };

  const emptyText =
    data.length === 0
      ? t('lineCatalog.empty')
      : t('lineCatalog.emptyFiltered');

  const columns: EditableColumn[] = [
    {
      title: t('lineCatalog.name'),
      dataIndex: 'nome',
      key: 'nome',
      editable: true,
      sorter: (a, b) => a.nome.localeCompare(b.nome),
    },
    {
      title: t('lineCatalog.description'),
      dataIndex: 'descricao',
      key: 'descricao',
      editable: true,
      render: (value?: string) => value || '-',
    },
    {
      title: t('lineCatalog.linkedMotorcycles'),
      key: 'motosVinculadas',
      width: 170,
      align: 'center',
      render: (_: unknown, record: LinhaData) => countModelosVinculados(record.id),
    },
    {
      title: t('lineCatalog.status'),
      key: 'status',
      width: 140,
      align: 'center',
      render: (_: unknown, record: LinhaData) => {
        const editable = isEditing(record);
        return editable ? (
          <Tag color={record.ativo ? 'green' : 'red'}>
            {record.ativo ? t('status.activeSingle') : t('status.inactiveSingle')}
          </Tag>
        ) : (
          <Switch
            checked={record.ativo}
            checkedChildren={t('status.activeSingle')}
            disabled={editingKey !== '' || savingKey === record.key}
            loading={savingKey === record.key}
            onChange={() => toggleStatus(record)}
            unCheckedChildren={t('status.inactiveSingle')}
          />
        );
      },
    },
    {
      title: t('lineCatalog.actions'),
      key: 'actions',
      width: 150,
      render: (_: unknown, record: LinhaData) => {
        const editable = isEditing(record);
        return editable ? (
          <Space>
            <Button type="link" loading={savingKey === record.key} onClick={() => save(record)}>
              {t('lineCatalog.save')}
            </Button>
            <Button type="link" danger onClick={cancel}>
              {t('lineCatalog.cancel')}
            </Button>
          </Space>
        ) : (
          <Button
            type="link"
            icon={<EditOutlined />}
            disabled={editingKey !== ''}
            onClick={() => edit(record)}
          >
            {t('lineCatalog.edit')}
          </Button>
        );
      },
    },
  ];

  const mergedColumns = columns.map((col) => {
    if (!col.editable) {
      return col;
    }

    return {
      ...col,
      onCell: (record: LinhaData) => ({
        record,
        dataIndex: col.dataIndex,
        title: col.title as string,
        editing: isEditing(record),
      }),
    };
  });

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
              title: t('lineCatalog.title'),
            },
          ]}
        />

        <Title level={2} style={{ margin: 0 }}>
          {t('lineCatalog.title')}
        </Title>

        <Flex gap="middle" align="center" wrap="wrap">
          <Input
            allowClear
            placeholder={t('lineCatalog.searchPlaceholder')}
            prefix={<SearchOutlined />}
            style={{ width: 300 }}
            value={searchText}
            onChange={(event) => setSearchText(event.target.value)}
          />

          <Select
            options={[
              { value: 'active', label: t('status.active') },
              { value: 'inactive', label: t('status.inactive') },
              { value: 'all', label: t('status.all') },
            ]}
            placeholder={t('status.placeholder')}
            style={{ width: 140 }}
            value={statusFilter}
            onChange={setStatusFilter}
          />

          <Flex gap="small" style={{ marginLeft: 'auto' }}>
            <Button onClick={handleClearFilters}>{t('lineCatalog.clear')}</Button>
            <Button icon={<ReloadOutlined />} loading={loading} onClick={fetchLinhas}>
              {t('lineCatalog.refresh')}
            </Button>
          </Flex>
        </Flex>
      </Flex>

      <div
        style={{
          background: token.colorBgContainer,
          border: `1px solid ${token.colorBorderSecondary}`,
          borderRadius: token.borderRadiusLG,
          padding: token.paddingLG,
        }}
      >
        <Spin spinning={loading}>
          <Space direction="vertical" size="middle" style={{ width: '100%' }}>
            <Flex justify="space-between" align="center">
              <span>{t('lineCatalog.total', { count: filteredData.length })}</span>
              <Button type="primary" onClick={onNavigateToForm}>
                {t('lineCatalog.add')}
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
                columns={mergedColumns}
                dataSource={filteredData}
                locale={{
                  emptyText: (
                    <Empty
                      image={Empty.PRESENTED_IMAGE_SIMPLE}
                      description={emptyText}
                    />
                  ),
                }}
                pagination={{
                  onChange: () => setEditingKey(''),
                  pageSize: 10,
                }}
                rowClassName="editable-row"
                rowKey="id"
              />
            </Form>
          </Space>
        </Spin>
      </div>
    </Flex>
  );
}
