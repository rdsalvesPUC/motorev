import { Typography, Form, Input, InputNumber, Select, Button, Space, message, Card } from 'antd';
import { ToolOutlined, ArrowLeftOutlined } from '@ant-design/icons';
import DashboardBreadcrumb from '../common/DashboardBreadcrumb';
import { useForm, Controller } from 'react-hook-form';

const { Title } = Typography;

interface FormServicoProps {
  onCancel: () => void;
}

export interface ServicoFormData {
  codigo: string;
  nome: string;
  categoria: string;
  tempoEstimado: string;
  preco: number;
  descricao: string;
}

export default function FormServico({ onCancel }: FormServicoProps) {
  const { control, handleSubmit, reset, formState: { errors } } = useForm<ServicoFormData>({
    defaultValues: {
      codigo: '',
      nome: '',
      categoria: '',
      tempoEstimado: '',
      preco: 0,
      descricao: '',
    }
  });

  const handleFormSubmit = (data: ServicoFormData) => {
    try {
      message.success('Serviço cadastrado com sucesso!');
      reset();
    } catch (error) {
      message.error('Erro ao cadastrar serviço');
    }
  };

  const handleCancel = () => {
    reset();
    onCancel();
  };

  return (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <Space direction="vertical" size="middle" style={{ width: '100%' }}>
        <DashboardBreadcrumb
          userType="concessionaria"
          items={[
            {
              title: 'Catálogo de Serviços',
              icon: <ToolOutlined />,
              path: '/catalogos/servicos',
            },
            {
              title: 'Novo Serviço',
            },
          ]}
        />

        <Space align="center">
          <Button icon={<ArrowLeftOutlined />} onClick={handleCancel}>
            Voltar
          </Button>
          <Title level={2} style={{ margin: 0 }}>
            Cadastrar Novo Serviço
          </Title>
        </Space>
      </Space>

      <Card>
        <Form layout="vertical" onFinish={handleSubmit(handleFormSubmit)}>
          <Form.Item
            label="Código do Serviço"
            validateStatus={errors.codigo ? 'error' : ''}
            help={errors.codigo?.message}
            required
          >
            <Controller
              name="codigo"
              control={control}
              rules={{ required: 'Código é obrigatório' }}
              render={({ field }) => (
                <Input {...field} placeholder="Ex: S001" size="large" />
              )}
            />
          </Form.Item>

          <Form.Item
            label="Nome do Serviço"
            validateStatus={errors.nome ? 'error' : ''}
            help={errors.nome?.message}
            required
          >
            <Controller
              name="nome"
              control={control}
              rules={{ required: 'Nome é obrigatório' }}
              render={({ field }) => (
                <Input {...field} placeholder="Ex: Troca de Óleo" size="large" />
              )}
            />
          </Form.Item>

          <Form.Item
            label="Categoria"
            validateStatus={errors.categoria ? 'error' : ''}
            help={errors.categoria?.message}
            required
          >
            <Controller
              name="categoria"
              control={control}
              rules={{ required: 'Categoria é obrigatória' }}
              render={({ field }) => (
                <Select
                  {...field}
                  placeholder="Selecione a categoria"
                  size="large"
                  options={[
                    { value: 'Manutenção', label: 'Manutenção' },
                    { value: 'Suspensão', label: 'Suspensão' },
                    { value: 'Freios', label: 'Freios' },
                    { value: 'Motor', label: 'Motor' },
                    { value: 'Elétrica', label: 'Elétrica' },
                    { value: 'Transmissão', label: 'Transmissão' },
                  ]}
                />
              )}
            />
          </Form.Item>

          <Form.Item
            label="Tempo Estimado"
            validateStatus={errors.tempoEstimado ? 'error' : ''}
            help={errors.tempoEstimado?.message}
            required
          >
            <Controller
              name="tempoEstimado"
              control={control}
              rules={{ required: 'Tempo estimado é obrigatório' }}
              render={({ field }) => (
                <Input {...field} placeholder="Ex: 30 min" size="large" />
              )}
            />
          </Form.Item>

          <Form.Item
            label="Preço (R$)"
            validateStatus={errors.preco ? 'error' : ''}
            help={errors.preco?.message}
            required
          >
            <Controller
              name="preco"
              control={control}
              rules={{
                required: 'Preço é obrigatório',
                min: { value: 0.01, message: 'Preço deve ser maior que 0' }
              }}
              render={({ field }) => (
                <InputNumber
                  {...field}
                  style={{ width: '100%' }}
                  placeholder="80.00"
                  min={0}
                  step={10}
                  prefix="R$"
                  precision={2}
                  size="large"
                />
              )}
            />
          </Form.Item>

          <Form.Item
            label="Descrição"
            validateStatus={errors.descricao ? 'error' : ''}
            help={errors.descricao?.message}
            required
          >
            <Controller
              name="descricao"
              control={control}
              rules={{ required: 'Descrição é obrigatória' }}
              render={({ field }) => (
                <Input.TextArea
                  {...field}
                  rows={4}
                  placeholder="Descreva o serviço..."
                  size="large"
                />
              )}
            />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0 }}>
            <Space style={{ width: '100%', justifyContent: 'flex-end' }}>
              <Button size="large" onClick={handleCancel}>
                Cancelar
              </Button>
              <Button type="primary" size="large" htmlType="submit">
                Cadastrar Serviço
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>
    </Space>
  );
}
