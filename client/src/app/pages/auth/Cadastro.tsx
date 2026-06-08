import { Form, Input, Button, Tabs, Layout, Typography, Card, Space, message } from 'antd';
import { UserOutlined, LockOutlined, ShopOutlined, MailOutlined, IdcardOutlined, PhoneOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { useState, useEffect } from 'react';
import { clienteService } from '@/app/services/clienteService';
import { concessionariaService } from '@/app/services/concessionariaService';
import { viaCepService } from '@/app/services/viaCepService';
import { formatCEP, formatCNPJ, formatCPF, formatPhone } from '@/app/utils/formatters';
import { validateCNPJ, validateCPF, CPF_REGEX, CNPJ_REGEX, PHONE_REGEX, CEP_REGEX, UF_REGEX } from '@/app/utils/validators';
import { t } from '@/app/i18n';
import LanguageSelector from '@/app/components/layout/LanguageSelector';
import { PATHS } from '@/app/paths';

const { Content } = Layout;
const { Title, Text, Link } = Typography;

export default function Cadastro() {
  const navigate = useNavigate();
  const [formCliente] = Form.useForm();
  const [formConcessionaria] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [, forceUpdate] = useState({});

  const revalidateTouchedFields = (form: any) => {
    const touchedFieldNames = form
      .getFieldsError()
      .map(({ name }: { name: (string | number)[] }) => name)
      .filter((name: (string | number)[]) => form.isFieldTouched(name));

    if (touchedFieldNames.length > 0) {
      form.validateFields(touchedFieldNames).catch(() => {});
    }
  };

  useEffect(() => {
    const handleLangChange = () => {
      forceUpdate({});
      // Re-valida campos que já foram tocados para atualizar as mensagens de tradução
      revalidateTouchedFields(formCliente);
      revalidateTouchedFields(formConcessionaria);
    };
    window.addEventListener('languagechange', handleLangChange);
    return () => window.removeEventListener('languagechange', handleLangChange);
  }, [formCliente, formConcessionaria]);

  const handleClienteCadastro = async (values: any) => {
    try {
      setLoading(true);
      await clienteService.register({
        nome: values.nomeProprietario,
        email: values.email,
        cpf: values.cpf,
        telefone: values.cel,
        password: values.senha
      });
      message.success(t('cadastro.cliente.success'));
      navigate(PATHS.LOGIN);
    } catch (error: any) {
      if (error.status === 409) {
        message.error(t('cadastro.error.conflict'));
      } else if (error.status === 400) {
        message.error(t('cadastro.error.validation'));
      } else {
        message.error(t('cadastro.error'));
      }
    } finally {
      setLoading(false);
    }
  };

  const handleConcessionariaCadastro = async (values: any) => {
    try {
      setLoading(true);
      await concessionariaService.register({
        nome: values.nomeConcessionaria,
        email: values.email,
        password: values.senha,
        cnpj: values.cnpj,
        telefone: values.tel,
      });
      message.success(t('cadastro.concessionaria.success'));
      navigate(PATHS.LOGIN);
    } catch (error: any) {
      if (error.status === 409) {
        message.error(t('cadastro.error.conflict'));
      } else if (error.status === 400) {
        message.error(t('cadastro.error.validation'));
      } else {
        message.error(t('cadastro.error'));
      }
    } finally {
      setLoading(false);
    }
  };

  const clienteTab = (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <Form
        form={formCliente}
        name="cliente_cadastro"
        onFinish={handleClienteCadastro}
        layout="vertical"
        size="large"
      >
        <Form.Item
          label={t('cadastro.cliente.nome.label')}
          name="nomeProprietario"
          rules={[{ required: true, message: t('cadastro.cliente.nome.required') }]}
        >
          <Input prefix={<UserOutlined />} placeholder={t('cadastro.cliente.nome.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.cliente.cpf.label')}
          name="cpf"
          normalize={formatCPF}
          rules={[
            { required: true, message: t('cadastro.cliente.cpf.required') },
            {
              validator: (_, value) => {
                if (!value || (CPF_REGEX.test(value) && validateCPF(value))) {
                  return Promise.resolve();
                }
                return Promise.reject(new Error(t('cadastro.cliente.cpf.invalid')));
              }
            }
          ]}
        >
          <Input prefix={<IdcardOutlined />} placeholder={t('cadastro.cliente.cpf.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.email.label')}
          name="email"
          rules={[
            { required: true, message: t('cadastro.email.required') },
            { type: 'email', message: t('cadastro.email.invalid') }
          ]}
        >
          <Input prefix={<MailOutlined />} placeholder={t('cadastro.email.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.celular.label')}
          name="cel"
          normalize={formatPhone}
          rules={[
            { required: true, message: t('cadastro.celular.required') },
            { pattern: PHONE_REGEX, message: t('cadastro.celular.invalid') }
          ]}
        >
          <Input prefix={<PhoneOutlined />} placeholder={t('cadastro.celular.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.senha.label')}
          name="senha"
          rules={[
            { required: true, message: t('cadastro.senha.required') },
            { min: 6, message: t('cadastro.senha.min') },
            { pattern: /[A-Z]/, message: t('cadastro.senha.uppercase') },
            { pattern: /[a-z]/, message: t('cadastro.senha.lowercase') },
            { pattern: /[0-9]/, message: t('cadastro.senha.number') },
            { pattern: /[^A-Za-z0-9]/, message: t('cadastro.senha.special') }
          ]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder={t('cadastro.senha.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.confirmarSenha.label')}
          name="confirmarSenha"
          dependencies={['senha']}
          rules={[
            { required: true, message: t('cadastro.confirmarSenha.required') },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value || getFieldValue('senha') === value) {
                  return Promise.resolve();
                }
                return Promise.reject(new Error(t('cadastro.confirmarSenha.match')));
              },
            }),
          ]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder={t('cadastro.confirmarSenha.placeholder')} />
        </Form.Item>

        <Form.Item>
          <Button type="primary" htmlType="submit" block size="large" loading={loading}>
            {t('cadastro.cliente.submit')}
          </Button>
        </Form.Item>
      </Form>
    </Space>
  );

  const concessionariaTab = (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <Form
        form={formConcessionaria}
        name="concessionaria_cadastro"
        onFinish={handleConcessionariaCadastro}
        layout="vertical"
        size="large"
      >
        <Form.Item
          label={t('cadastro.concessionaria.nome.label')}
          name="nomeConcessionaria"
          rules={[{ required: true, message: t('cadastro.concessionaria.nome.required') }]}
        >
          <Input prefix={<ShopOutlined />} placeholder={t('cadastro.concessionaria.nome.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.concessionaria.cnpj.label')}
          name="cnpj"
          normalize={formatCNPJ}
          rules={[
            { required: true, message: t('cadastro.concessionaria.cnpj.required') },
            {
              validator: (_, value) => {
                if (!value || (CNPJ_REGEX.test(value) && validateCNPJ(value))) {
                  return Promise.resolve();
                }
                return Promise.reject(new Error(t('cadastro.concessionaria.cnpj.invalid')));
              }
            }
          ]}
        >
          <Input prefix={<IdcardOutlined />} placeholder={t('cadastro.concessionaria.cnpj.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.email.label')}
          name="email"
          rules={[
            { required: true, message: t('cadastro.email.required') },
            { type: 'email', message: t('cadastro.email.invalid') }
          ]}
        >
          <Input prefix={<MailOutlined />} placeholder={t('cadastro.concessionaria.email.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.concessionaria.telefone.label')}
          name="tel"
          normalize={formatPhone}
          rules={[
            { required: true, message: t('cadastro.concessionaria.telefone.required') },
            { pattern: PHONE_REGEX, message: t('cadastro.concessionaria.telefone.invalid') }
          ]}
        >
          <Input prefix={<PhoneOutlined />} placeholder={t('cadastro.concessionaria.telefone.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.senha.label')}
          name="senha"
          rules={[
            { required: true, message: t('cadastro.senha.required') },
            { min: 6, message: t('cadastro.senha.min') },
            { pattern: /[A-Z]/, message: t('cadastro.senha.uppercase') },
            { pattern: /[a-z]/, message: t('cadastro.senha.lowercase') },
            { pattern: /[0-9]/, message: t('cadastro.senha.number') },
            { pattern: /[^A-Za-z0-9]/, message: t('cadastro.senha.special') }
          ]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder={t('cadastro.senha.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.confirmarSenha.label')}
          name="confirmarSenha"
          dependencies={['senha']}
          rules={[
            { required: true, message: t('cadastro.confirmarSenha.required') },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value || getFieldValue('senha') === value) {
                  return Promise.resolve();
                }
                return Promise.reject(new Error(t('cadastro.confirmarSenha.match')));
              },
            }),
          ]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder={t('cadastro.confirmarSenha.placeholder')} />
        </Form.Item>

        <Form.Item>
          <Button type="primary" htmlType="submit" block size="large" loading={loading}>
            {t('cadastro.concessionaria.submit')}
          </Button>
        </Form.Item>
      </Form>
    </Space>
  );

  const items = [
    {
      key: 'cliente',
      label: t('cadastro.cliente.tab'),
      children: clienteTab,
    },
    {
      key: 'concessionaria',
      label: t('cadastro.concessionaria.tab'),
      children: concessionariaTab,
    },
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Content style={{ display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', padding: '24px' }}>
        <div style={{ position: 'absolute', top: 24, right: 24 }}>
          <LanguageSelector />
        </div>

        <Space direction="vertical" size="large" style={{ width: '100%', maxWidth: '480px' }}>
          <Space direction="vertical" size="small" style={{ width: '100%', textAlign: 'center' }}>
            <Title level={2} style={{ margin: 0 }}>{t('cadastro.title')}</Title>
            <Text type="secondary">{t('cadastro.subtitle')}</Text>
          </Space>

          <Card>
            <Tabs items={items} centered size="large" />
          </Card>

          <Space direction="vertical" size="small" style={{ width: '100%', textAlign: 'center' }}>
            <Text type="secondary">
              {t('cadastro.hasAccount')} <Link onClick={() => navigate(PATHS.LOGIN)}>{t('cadastro.login')}</Link>
            </Text>
            <Link onClick={() => navigate(PATHS.HOME)}>{t('cadastro.back')}</Link>
          </Space>
        </Space>
      </Content>
    </Layout>
  );
}
