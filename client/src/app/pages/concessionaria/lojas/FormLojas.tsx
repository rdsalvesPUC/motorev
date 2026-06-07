import { useEffect, useState } from 'react';
import { useParams } from 'react-router';
import { Typography, Form, Input, Button, Space, message, Card, Spin, Tag } from 'antd';
import { ShopOutlined, ArrowLeftOutlined } from '@ant-design/icons';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { concessionariaService } from '@/app/services/concessionariaService';
import { viaCepService } from '@/app/services/viaCepService';
import { LojaRequest } from '@/app/models/LojaRequest';
import { handleApiError } from '@/app/utils/errorHandler';
import { formatCEP, formatCNPJ } from '@/app/utils/formatters';
import { CEP_REGEX, CNPJ_REGEX, UF_REGEX, validateCNPJ } from '@/app/utils/validators';
import { PATH_SEGMENTS } from '@/app/paths';

const { Title } = Typography;

interface LojasCreateProps {
  onBack: () => void;
}

export default function FormLojas({ onBack }: LojasCreateProps) {
  const [form] = Form.useForm<LojaRequest>();
  const { id } = useParams();
  const lojaId = id ? Number(id) : undefined;
  const isEditing = lojaId !== undefined && !Number.isNaN(lojaId);
  const [concessionariaId, setConcessionariaId] = useState<number>();
  const [loading, setLoading] = useState(false);
  const [buscandoCep, setBuscandoCep] = useState(false);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const concessionaria = await concessionariaService.getMe();
        setConcessionariaId(concessionaria.id);

        if (isEditing && lojaId) {
          const loja = await concessionariaService.getLojaById(concessionaria.id, lojaId);
          form.setFieldsValue({
            nome: loja.nome,
            cnpj: loja.cnpj,
            cep: loja.cep,
            logradouro: loja.logradouro,
            numero: loja.numero,
            bairro: loja.bairro,
            cidade: loja.cidade,
            uf: loja.uf,
          });
        }
      } catch (error) {
        handleApiError(error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [form, isEditing, lojaId]);

  const handleSubmit = async (values: LojaRequest) => {
    if (!concessionariaId) return;

    const payload = {
      ...values,
      uf: values.uf.toUpperCase(),
    };

    try {
      setLoading(true);
      if (isEditing && lojaId) {
        await concessionariaService.updateLoja(concessionariaId, lojaId, payload);
        message.success('Loja atualizada com sucesso!');
      } else {
        await concessionariaService.createLoja(concessionariaId, payload);
        message.success('Loja criada com sucesso!');
      }
      form.resetFields();
      onBack();
    } catch (error) {
      handleApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    form.resetFields();
    onBack();
  };

  const handleBuscarCep = async () => {
    const cep = form.getFieldValue('cep');
    const digits = String(cep || '').replace(/\D/g, '');
    if (digits.length === 0) return;
    if (digits.length !== 8) {
      message.warning('Informe um CEP com 8 digitos');
      return;
    }

    try {
      setBuscandoCep(true);
      const endereco = await viaCepService.getAddressByCep(cep);
      form.setFieldsValue({
        cep: formatCEP(endereco.cep),
        logradouro: endereco.logradouro,
        bairro: endereco.bairro,
        cidade: endereco.cidade,
        uf: endereco.uf,
      });
    } catch (error: any) {
      message.warning(error.message || 'CEP nao encontrado');
    } finally {
      setBuscandoCep(false);
    }
  };

  return (
    <Spin spinning={loading}>
      <Space orientation="vertical" size="large" style={{ width: '100%' }}>
        <Space orientation="vertical" size="middle" style={{ width: '100%' }}>
          <DashboardBreadcrumb
            userType="concessionaria"
            items={[
              {
                title: 'Lojas',
                icon: <ShopOutlined />,
                path: PATH_SEGMENTS.CONCESSIONARIA_LOJAS,
              },
              {
                title: isEditing ? 'Editar Loja' : 'Adicionar Loja',
              },
            ]}
          />

          <Space align="center">
            <Button icon={<ArrowLeftOutlined />} onClick={handleCancel}>
              Voltar
            </Button>
            <Title level={2} style={{ margin: 0 }}>
              {isEditing ? 'Editar Loja' : 'Adicionar Loja'}
            </Title>
          </Space>
        </Space>

        <Card>
          <Form form={form} layout="vertical" onFinish={handleSubmit}>
            <Form.Item label="Tipo">
              <Tag color="blue">Filial</Tag>
            </Form.Item>

            <Form.Item
              label="Nome da Loja"
              name="nome"
              rules={[{ required: true, message: 'Informe o nome da loja' }]}
            >
              <Input placeholder="Ex: Moto Center - Unidade Sul" />
            </Form.Item>

            <Form.Item
              label="CNPJ"
              name="cnpj"
              normalize={formatCNPJ}
              rules={[
                { required: true, message: 'Informe o CNPJ' },
                {
                  validator: (_, value) => {
                    if (!value || (CNPJ_REGEX.test(value) && validateCNPJ(value))) {
                      return Promise.resolve();
                    }
                    return Promise.reject(new Error('CNPJ invalido'));
                  },
                },
              ]}
            >
              <Input placeholder="Ex: 12.345.678/0001-00" />
            </Form.Item>

            <Title level={5}>Endereco</Title>

            <Form.Item
              label="CEP"
              name="cep"
              normalize={formatCEP}
              rules={[
                { required: true, message: 'Informe o CEP' },
                { pattern: CEP_REGEX, message: 'CEP invalido' },
              ]}
            >
              <Input placeholder="Ex: 01310-100" onBlur={handleBuscarCep} />
            </Form.Item>

            <Form.Item
              label="Rua / Avenida"
              name="logradouro"
              rules={[{ required: true, message: 'Informe o logradouro' }]}
            >
              <Input placeholder="Ex: Av. Paulista" disabled={buscandoCep} />
            </Form.Item>

            <Form.Item
              label="Numero"
              name="numero"
              rules={[{ required: true, message: 'Informe o numero' }]}
            >
              <Input placeholder="Ex: 1000" />
            </Form.Item>

            <Form.Item
              label="Bairro"
              name="bairro"
              rules={[{ required: true, message: 'Informe o bairro' }]}
            >
              <Input placeholder="Ex: Bela Vista" disabled={buscandoCep} />
            </Form.Item>

            <Form.Item
              label="Cidade"
              name="cidade"
              rules={[{ required: true, message: 'Informe a cidade' }]}
            >
              <Input placeholder="Ex: Sao Paulo" disabled={buscandoCep} />
            </Form.Item>

            <Form.Item
              label="UF"
              name="uf"
              rules={[
                { required: true, message: 'Informe a UF' },
                { pattern: UF_REGEX, message: 'UF deve conter 2 letras' },
              ]}
            >
              <Input
                placeholder="Ex: SP"
                maxLength={2}
                disabled={buscandoCep}
                onChange={(event) => form.setFieldValue('uf', event.target.value.toUpperCase())}
              />
            </Form.Item>

            <Form.Item style={{ marginBottom: 0 }}>
              <Space style={{ width: '100%', justifyContent: 'flex-end' }}>
                <Button onClick={handleCancel}>Cancelar</Button>
                <Button type="primary" htmlType="submit" loading={loading}>
                  {isEditing ? 'Salvar Alteracoes' : 'Adicionar Loja'}
                </Button>
              </Space>
            </Form.Item>
          </Form>
        </Card>
      </Space>
    </Spin>
  );
}
