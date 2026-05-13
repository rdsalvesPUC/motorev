
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import { Typography, Input, Select, Button, Table, Space, Flex, Form, Popconfirm, message, Spin, Tag, InputNumber, Empty } from 'antd';
import { ToolOutlined, SearchOutlined, EditOutlined, DeleteOutlined, SaveOutlined, CloseOutlined, EyeOutlined } from '@ant-design/icons';
import type { ColumnType } from 'antd/es/table';
import DashboardBreadcrumb from '../common/DashboardBreadcrumb';
import { servicoService } from '../../services/servicoService';
import { Servico } from '../../models/Servico';
import { t } from '../../i18n';
import { ApiError } from '../../services/http';
import { handleApiError } from '../../utils/errorHandler';

const { Title } = Typography;

interface ServicoData extends Servico {
  key: string;
}

interface CatalogoServicosProps {
  onNavigateToForm?: () => void;
}

interface EditableColumn extends ColumnType<ServicoData> {
  editable?: boolean;
}

interface EditableCellProps {
  editing: boolean;
  dataIndex: string;
  cellTitle: any;
  record: ServicoData;
  children: React.ReactNode;
  form: any;
}

const EditableCell: React.FC<EditableCellProps> = ({
  editing,
  dataIndex,
  cellTitle,
  children,
  form,
  ...restProps
}) => {
  const inputNodeMap: Record<string, React.ReactNode> = {
    codigo: (
      <Input
        style={{ textTransform: 'uppercase' }}
        onChange={(e) => {
          form.setFieldsValue({ codigo: e.target.value.toUpperCase() });
        }}
      />
    ),
    descricao: <Input.TextArea rows={2} />,
    tempoEstimado: (
      <InputNumber
        min={1}
        max={480}
        style={{ width: '100%' }}
        parser={(value) => value?.replace(/[^\d]/g, '') as any}
      />
    ),
    custo: (
      <InputNumber
        min={0}
        max={100000}
        style={{ width: '100%' }}
        decimalSeparator=","
        precision={2}
        parser={(value) => value?.replace(/[^\d,]/g, '').replace(',', '.') as any}
        formatter={(value) =>
          value ? `${value}`.replace('.', ',').replace(/\B(?=(\d{3})+(?!\d))/g, '.') : ''
        }
      />
    ),
    categoria: (
      <Select
        style={{ width: '100%' }}
        options={[
          { value: 'Verificacao', label: t('serviceCatalog.category.verificacao') },
          { value: 'Ajuste', label: t('serviceCatalog.category.ajuste') },
          { value: 'Limpeza', label: t('serviceCatalog.category.limpeza') },
          { value: 'Troca', label: t('serviceCatalog.category.troca') },
        ]}
      />
    ),
  };

  const inputNode = inputNodeMap[dataIndex] || <Input />;

  return (
    <td {...restProps}>
      {editing ? (
        <Form.Item
          name={dataIndex}
          style={{ margin: 0 }}
          rules={[
            {
              required: true,
              message: t('serviceCatalog.enterField', { field: cellTitle }),
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

export default function CatalogoServicos({ onNavigateToForm }: CatalogoServicosProps) {
  const [form] = Form.useForm();
  const navigate = useNavigate();
  const [editingKey, setEditingKey] = useState('');
  const [data, setData] = useState<ServicoData[]>([]);
  const [filteredData, setFilteredData] = useState<ServicoData[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchText, setSearchText] = useState('');
  const [categoryFilter, setCategoryFilter] = useState<string | undefined>(undefined);

  const fetchServicos = async (categoria?: string) => {
    try {
      setLoading(true);
      const servicos = await servicoService.getAll(categoria);
      const mappedData = servicos.map(s => ({ ...s, key: s.id.toString() }));
      setData(mappedData);

      if (searchText) {
        const lowerSearch = searchText.toLowerCase();
        setFilteredData(mappedData.filter(item => 
          item.nome.toLowerCase().includes(lowerSearch) || 
          item.codigo.toLowerCase().includes(lowerSearch)
        ));
      } else {
        setFilteredData(mappedData);
      }
    } catch (error) {
      handleApiError(error, 'error.fetchServices');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchServicos();
  }, []);

  const handleApplyFilters = () => {
    fetchServicos(categoryFilter);
  };

  const handleClearFilters = () => {
    setSearchText('');
    setCategoryFilter(undefined);
    fetchServicos();
  };

  const isEditing = (record: ServicoData) => record.key === editingKey;

  const edit = (record: ServicoData) => {
    form.setFieldsValue({ ...record });
    setEditingKey(record.key);
  };

  const cancel = () => {
    setEditingKey('');
  };

  const save = async (key: string) => {
    try {
      const row = await form.validateFields();
      const newData = [...data];
      const index = newData.findIndex((item) => key === item.key);

      if (index > -1) {
        const item = newData[index];
        const updatedItem = { ...item, ...row };
        
        setLoading(true);
        await servicoService.update(item.id, row);
        
        newData.splice(index, 1, updatedItem);
        setData(newData);
        setEditingKey('');
        
        // Update filtered data as well
        const filteredIndex = filteredData.findIndex(item => item.key === key);
        if (filteredIndex > -1) {
          const newFilteredData = [...filteredData];
          newFilteredData.splice(filteredIndex, 1, updatedItem);
          setFilteredData(newFilteredData);
        }

        message.success(t('serviceUpdatedSuccess'));
      }
    } catch (errInfo) {
      handleApiError(errInfo);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (key: string) => {
    const item = data.find(i => i.key === key);
    if (!item) return;

    try {
      setLoading(true);
      await servicoService.delete(item.id);
      
      const newData = data.filter((item) => item.key !== key);
      setData(newData);
      setFilteredData(filteredData.filter(item => item.key !== key));
      message.success(t('serviceDeletedSuccess'));
    } catch (error) {
      handleApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const showDetails = (id: number) => {
    navigate(`/dashboard/concessionaria/catalogos-servicos/${id}`);
  };

  const getCategoryColor = (categoria: string) => {
    const colors: Record<string, string> = {
      'Verificacao': 'blue',
      'Ajuste': 'orange',
      'Limpeza': 'green',
      'Troca': 'red'
    };
    return colors[categoria] || 'default';
  };

  const columns: EditableColumn[] = [
    {
      title: t('serviceCatalog.code'),
      dataIndex: 'codigo',
      key: 'codigo',
      editable: true,
    },
    {
      title: t('serviceCatalog.serviceName'),
      dataIndex: 'nome',
      key: 'nome',
      editable: true,
    },
    {
      title: t('serviceCatalog.category'),
      dataIndex: 'categoria',
      key: 'categoria',
      editable: true,
      render: (categoria: string) => (
        <Tag color={getCategoryColor(categoria)}>
          {t(`serviceCatalog.category.${categoria.toLowerCase()}`)}
        </Tag>
      ),
    },
    {
      title: t('serviceCatalog.estimatedTime'),
      dataIndex: 'tempoEstimado',
      key: 'tempoEstimado',
      editable: true,
      render: (tempo: number) => `${tempo} ${t('minutes')}`,
    },
    {
      title: t('serviceCatalog.price'),
      dataIndex: 'custo',
      key: 'custo',
      editable: true,
      render: (custo: number) => `R$ ${custo.toFixed(2)}`,
    },
    {
      title: t('serviceCatalog.description'),
      dataIndex: 'descricao',
      key: 'descricao',
      editable: true,
    },
    {
      title: t('serviceCatalog.actions'),
      key: 'actions',
      width: 180,
      render: (_: any, record: ServicoData) => {
        const editable = isEditing(record);
        return editable ? (
          <Space size="small">
            <Button
              type="link"
              icon={<SaveOutlined />}
              onClick={() => save(record.key)}
            >
              {t('serviceCatalog.save')}
            </Button>
            <Button
              type="link"
              icon={<CloseOutlined />}
              onClick={cancel}
            >
              {t('serviceCatalog.cancel')}
            </Button>
          </Space>
        ) : (
          <Space size="small">
            <Button
              type="link"
              icon={<EyeOutlined />}
              onClick={() => showDetails(record.id)}
            >
              {t('serviceCatalog.details')}
            </Button>
            <Button
              type="link"
              icon={<EditOutlined />}
              disabled={editingKey !== ''}
              onClick={() => edit(record)}
            >
              {t('serviceCatalog.edit')}
            </Button>
            <Popconfirm
              title={t('confirmDelete')}
              onConfirm={() => handleDelete(record.key)}
              okText={t('yes')}
              cancelText={t('no')}
            >
              <Button
                type="link"
                danger
                icon={<DeleteOutlined />}
                disabled={editingKey !== ''}
              >
                {t('serviceCatalog.delete')}
              </Button>
            </Popconfirm>
          </Space>
        );
      },
    },
  ];

  const mergedColumns = columns.map((col) => {
    if (!col.editable) {
      return col;
    }
    const { editable, ...colWithoutEditable } = col;
    return {
      ...colWithoutEditable,
      onCell: (record: ServicoData) => ({
        record,
        dataIndex: col.dataIndex,
        cellTitle: col.title,
        editing: isEditing(record),
        form: form,
      }),
    };
  });

  return (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <Space direction="vertical" size="middle" style={{ width: '100%' }}>
        <DashboardBreadcrumb
          userType="concessionaria"
          items={[
            {
              title: t('serviceCatalog.title'),
              icon: <ToolOutlined />,
            },
          ]}
        />

        <Title level={2} style={{ margin: 0 }}>
          {t('serviceCatalog.title')}
        </Title>

        <Flex gap="middle" align="center" wrap="wrap">
          <Input
            placeholder={t('serviceCatalog.searchPlaceholder')}
            prefix={<SearchOutlined />}
            style={{ width: 300 }}
            value={searchText}
            onChange={(e) => setSearchText(e.target.value)}
            onPressEnter={handleApplyFilters}
          />

          <Select
            placeholder={t('serviceCatalog.category')}
            style={{ width: 150 }}
            value={categoryFilter}
            onChange={(value) => setCategoryFilter(value)}
            allowClear
            options={[
              { value: 'Verificacao', label: t('serviceCatalog.category.verificacao') },
              { value: 'Ajuste', label: t('serviceCatalog.category.ajuste') },
              { value: 'Limpeza', label: t('serviceCatalog.category.limpeza') },
              { value: 'Troca', label: t('serviceCatalog.category.troca') },
            ]}
          />

          <Flex gap="small" style={{ marginLeft: 'auto' }}>
            <Button onClick={handleClearFilters}>{t('serviceCatalog.clear')}</Button>
            <Button type="primary" onClick={handleApplyFilters}>{t('serviceCatalog.apply')}</Button>
          </Flex>
        </Flex>
      </Space>

      <div style={{ background: '#fff', padding: '24px', borderRadius: '8px' }}>
        <Spin spinning={loading}>
          <Space direction="vertical" size="middle" style={{ width: '100%' }}>
            <Flex justify="space-between" align="center">
              <span>{t('serviceCatalog.totalServices', { count: filteredData.length })}</span>
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
                columns={mergedColumns as ColumnType<ServicoData>[]}
                dataSource={filteredData}
                pagination={{
                  onChange: cancel,
                }}
                locale={{
                  emptyText: (
                    <Empty
                      image={Empty.PRESENTED_IMAGE_SIMPLE}
                      description={
                        <span>
                          {t('serviceCatalog.empty')}<br />
                          <small>{t('serviceCatalog.emptyDescription')}</small>
                        </span>
                      }
                    />
                  )
                }}
              />
            </Form>
          </Space>
        </Spin>
      </div>
    </Space>
  );
}
