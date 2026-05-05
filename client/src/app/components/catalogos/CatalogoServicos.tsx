import { useState } from 'react';
import { Typography, Input, Select, Button, Table, Space, Flex, Form, Popconfirm } from 'antd';
import { ToolOutlined, SearchOutlined, EditOutlined, DeleteOutlined, SaveOutlined, CloseOutlined } from '@ant-design/icons';
import type { ColumnType } from 'antd/es/table';
import DashboardBreadcrumb from '../common/DashboardBreadcrumb';

const { Title } = Typography;

interface ServicoData {
  key: string;
  codigo: string;
  nome: string;
  categoria: string;
  tempoEstimado: string;
  preco: string;
  descricao: string;
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
}

const EditableCell: React.FC<EditableCellProps> = ({
  editing,
  dataIndex,
  cellTitle,
  children,
  ...restProps
}) => {
  let inputNode = <Input />;

  if (dataIndex === 'descricao') {
    inputNode = <Input.TextArea rows={2} />;
  } else if (dataIndex === 'categoria') {
    inputNode = (
      <Select
        style={{ width: '100%' }}
        options={[
          { value: 'Manutenção', label: 'Manutenção' },
          { value: 'Suspensão', label: 'Suspensão' },
          { value: 'Freios', label: 'Freios' },
          { value: 'Motor', label: 'Motor' },
          { value: 'Elétrica', label: 'Elétrica' },
          { value: 'Transmissão', label: 'Transmissão' },
        ]}
      />
    );
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
              message: `Por favor, insira ${cellTitle}!`,
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
  const [editingKey, setEditingKey] = useState('');

  const [data, setData] = useState<ServicoData[]>([
    {
      key: '1',
      codigo: 'S001',
      nome: 'Troca de Óleo',
      categoria: 'Manutenção',
      tempoEstimado: '30 min',
      preco: 'R$ 80,00',
      descricao: 'Troca completa do óleo do motor',
    },
    {
      key: '2',
      codigo: 'S002',
      nome: 'Alinhamento',
      categoria: 'Suspensão',
      tempoEstimado: '1h',
      preco: 'R$ 120,00',
      descricao: 'Alinhamento e balanceamento',
    },
    {
      key: '3',
      codigo: 'S003',
      nome: 'Regulagem de Freios',
      categoria: 'Freios',
      tempoEstimado: '45 min',
      preco: 'R$ 100,00',
      descricao: 'Regulagem completa do sistema de freios',
    },
    {
      key: '4',
      codigo: 'S004',
      nome: 'Limpeza de Carburador',
      categoria: 'Motor',
      tempoEstimado: '2h',
      preco: 'R$ 250,00',
      descricao: 'Desmontagem e limpeza completa',
    },
  ]);

  const isEditing = (record: ServicoData) => record.key === editingKey;

  const edit = (record: ServicoData) => {
    form.setFieldsValue({
      codigo: record.codigo,
      nome: record.nome,
      categoria: record.categoria,
      tempoEstimado: record.tempoEstimado,
      preco: record.preco,
      descricao: record.descricao,
    });
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
        newData.splice(index, 1, { ...item, ...row });
        setData(newData);
        setEditingKey('');
      }
    } catch (errInfo) {
      console.log('Validate Failed:', errInfo);
    }
  };

  const handleDelete = (key: string) => {
    const newData = data.filter((item) => item.key !== key);
    setData(newData);
  };

  const columns: EditableColumn[] = [
    {
      title: 'Código',
      dataIndex: 'codigo',
      key: 'codigo',
      editable: true,
    },
    {
      title: 'Nome do Serviço',
      dataIndex: 'nome',
      key: 'nome',
      editable: true,
    },
    {
      title: 'Categoria',
      dataIndex: 'categoria',
      key: 'categoria',
      editable: true,
    },
    {
      title: 'Tempo Estimado',
      dataIndex: 'tempoEstimado',
      key: 'tempoEstimado',
      editable: true,
    },
    {
      title: 'Preço',
      dataIndex: 'preco',
      key: 'preco',
      editable: true,
    },
    {
      title: 'Descrição',
      dataIndex: 'descricao',
      key: 'descricao',
      editable: true,
    },
    {
      title: 'Ações',
      key: 'actions',
      width: 150,
      render: (_: any, record: ServicoData) => {
        const editable = isEditing(record);
        return editable ? (
          <Space size="small">
            <Button
              type="link"
              icon={<SaveOutlined />}
              onClick={() => save(record.key)}
            >
              Salvar
            </Button>
            <Button
              type="link"
              icon={<CloseOutlined />}
              onClick={cancel}
            >
              Cancelar
            </Button>
          </Space>
        ) : (
          <Space size="small">
            <Button
              type="link"
              icon={<EditOutlined />}
              disabled={editingKey !== ''}
              onClick={() => edit(record)}
            >
              Editar
            </Button>
            <Popconfirm
              title="Tem certeza que deseja excluir?"
              onConfirm={() => handleDelete(record.key)}
              okText="Sim"
              cancelText="Não"
            >
              <Button
                type="link"
                danger
                icon={<DeleteOutlined />}
                disabled={editingKey !== ''}
              >
                Excluir
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
              title: 'Catálogo de Serviços',
              icon: <ToolOutlined />,
            },
          ]}
        />

        <Title level={2} style={{ margin: 0 }}>
          Catálogo de Serviços
        </Title>

        <Flex gap="middle" align="center" wrap="wrap">
          <Input
            placeholder="Buscar serviços..."
            prefix={<SearchOutlined />}
            style={{ width: 300 }}
          />

          <Select
            placeholder="Categoria"
            style={{ width: 150 }}
            options={[
              { value: 'manutencao', label: 'Manutenção' },
              { value: 'suspensao', label: 'Suspensão' },
              { value: 'freios', label: 'Freios' },
              { value: 'motor', label: 'Motor' },
              { value: 'eletrica', label: 'Elétrica' },
            ]}
          />

          <Select
            placeholder="Tempo"
            style={{ width: 150 }}
            options={[
              { value: '30min', label: 'Até 30 min' },
              { value: '1h', label: 'Até 1h' },
              { value: '2h', label: 'Até 2h' },
              { value: 'mais', label: 'Mais de 2h' },
            ]}
          />

          <Select
            placeholder="Preço"
            style={{ width: 150 }}
            options={[
              { value: 'ate100', label: 'Até R$ 100' },
              { value: 'ate200', label: 'Até R$ 200' },
              { value: 'ate500', label: 'Até R$ 500' },
              { value: 'acima500', label: 'Acima de R$ 500' },
            ]}
          />

          <Flex gap="small" style={{ marginLeft: 'auto' }}>
            <Button>Limpar</Button>
            <Button type="primary">Aplicar</Button>
          </Flex>
        </Flex>
      </Space>

      <div style={{ background: '#fff', padding: '24px', borderRadius: '8px' }}>
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
          <Flex justify="space-between" align="center">
            <span>Total: {data.length} serviços</span>
            <Button type="primary" onClick={onNavigateToForm}>
              Adicionar Serviço
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
              dataSource={data}
              pagination={{
                onChange: cancel,
              }}
            />
          </Form>
        </Space>
      </div>
    </Space>
  );
}
