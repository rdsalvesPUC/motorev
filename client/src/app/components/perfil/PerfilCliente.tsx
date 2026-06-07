import { useEffect, useMemo, useState } from 'react';
import {
  Avatar,
  Breadcrumb,
  Button,
  Card,
  Descriptions,
  Divider,
  Flex,
  Form,
  Input,
  Modal,
  Radio,
  Skeleton,
  Space,
  Spin,
  Switch,
  Typography,
  message,
} from 'antd';
import {
  BulbOutlined,
  CloseOutlined,
  EditOutlined,
  EnvironmentOutlined,
  GlobalOutlined,
  HomeOutlined,
  IdcardOutlined,
  LockOutlined,
  MailOutlined,
  PhoneOutlined,
  PlusOutlined,
  SaveOutlined,
  SettingOutlined,
  UserOutlined,
} from '@ant-design/icons';
import { clienteService } from '../../services/clienteService';
import { tokenManager } from '../../services/tokenManager';
import { viaCepService } from '../../services/viaCepService';
import { useConfiguracoes, type Idioma } from '../../contexts/ConfiguracoesContext';
import { t } from '../../i18n';
import type {
  ClienteDadosPessoaisRequest,
  ClienteEndereco,
  ClienteEnderecoRequest,
  ClientePerfil,
} from '../../models/ClientePerfil';
import { formatCPF, formatPhone } from '../../utils/formatters';
import { PHONE_REGEX } from '../../utils/validators';

const { Title, Text } = Typography;

interface PerfilClienteProps {
  onProfileUpdated?: (perfil: ClientePerfil) => void;
}

interface SectionCardProps {
  title: string;
  icon: React.ReactNode;
  editing: boolean;
  saving?: boolean;
  onEdit: () => void;
  onSave: () => void;
  onCancel: () => void;
  children: React.ReactNode;
}

function normalizeText(value?: string | null) {
  return value?.trim() || '';
}

function formatCep(value?: string | null) {
  const digits = normalizeText(value).replace(/\D/g, '');
  if (digits.length !== 8) return normalizeText(value);
  return digits.replace(/(\d{5})(\d{3})/, '$1-$2');
}

function formatCepInput(value: string) {
  const digits = value.replace(/\D/g, '').slice(0, 8);
  if (digits.length <= 5) return digits;
  return digits.replace(/(\d{5})(\d{0,3})/, '$1-$2');
}

function hasEndereco(endereco?: ClienteEndereco | null) {
  if (!endereco) return false;
  return Object.values(endereco).some((value) => Boolean(normalizeText(value)));
}

function buildEnderecoDescription(endereco: ClienteEndereco) {
  const logradouro = normalizeText(endereco.logradouro);
  const numero = normalizeText(endereco.numero);
  const complemento = normalizeText(endereco.complemento);
  const bairro = normalizeText(endereco.bairro);
  const cidade = normalizeText(endereco.cidade);
  const uf = normalizeText(endereco.uf).toUpperCase();
  const cep = formatCep(endereco.cep);

  return {
    linha1: [logradouro, numero].filter(Boolean).join(', '),
    complemento,
    bairro,
    cidadeUf: [cidade, uf].filter(Boolean).join(' - '),
    cep,
  };
}

function SectionCard({
  title,
  icon,
  editing,
  saving,
  onEdit,
  onSave,
  onCancel,
  children,
}: SectionCardProps) {
  return (
    <Card
      title={
        <Flex align="center" gap={8}>
          {icon}
          <span>{title}</span>
        </Flex>
      }
      extra={
        editing ? (
          <Flex gap={8} wrap="wrap">
            <Button size="small" icon={<CloseOutlined />} onClick={onCancel} disabled={saving}>
              {t('perfil.actions.cancel')}
            </Button>
            <Button size="small" type="primary" icon={<SaveOutlined />} onClick={onSave} loading={saving}>
              {t('perfil.actions.save')}
            </Button>
          </Flex>
        ) : (
          <Button size="small" icon={<EditOutlined />} onClick={onEdit}>
            {t('perfil.actions.edit')}
          </Button>
        )
      }
    >
      {children}
    </Card>
  );
}

export default function PerfilCliente({ onProfileUpdated }: PerfilClienteProps) {
  const [profile, setProfile] = useState<ClientePerfil | null>(null);
  const [loading, setLoading] = useState(true);
  const [editingDados, setEditingDados] = useState(false);
  const [editingEndereco, setEditingEndereco] = useState(false);
  const [savingDados, setSavingDados] = useState(false);
  const [savingEndereco, setSavingEndereco] = useState(false);
  const [savingSenha, setSavingSenha] = useState(false);
  const [loadingCep, setLoadingCep] = useState(false);
  const [senhaModalOpen, setSenhaModalOpen] = useState(false);
  const { configuracoes, setIdioma, setTema } = useConfiguracoes();

  const [formDados] = Form.useForm<ClienteDadosPessoaisRequest>();
  const [formEndereco] = Form.useForm<ClienteEnderecoRequest>();
  const [formSenha] = Form.useForm();

  const enderecoDescription = useMemo(() => {
    if (!profile?.endereco) return null;
    return buildEnderecoDescription(profile.endereco);
  }, [profile?.endereco]);

  useEffect(() => {
    let mounted = true;

    const loadPerfil = async () => {
      try {
        const data = await clienteService.getPerfil();
        if (!mounted) return;
        setProfile(data);
        onProfileUpdated?.(data);
      } catch (error: any) {
        if (mounted) {
          message.error(error.message || t('perfil.loading.error'));
        }
      } finally {
        if (mounted) {
          setLoading(false);
        }
      }
    };

    loadPerfil();

    return () => {
      mounted = false;
    };
  }, [onProfileUpdated]);

  const updateProfile = (perfil: ClientePerfil) => {
    setProfile(perfil);
    tokenManager.updateUserData({ nome: perfil.nome, email: perfil.email });
    onProfileUpdated?.(perfil);
  };

  const handleEditDados = () => {
    if (!profile) return;
    formDados.setFieldsValue({
      nome: profile.nome,
      email: profile.email,
      telefone: formatPhone(normalizeText(profile.telefone)),
    });
    setEditingDados(true);
  };

  const handleSaveDados = async () => {
    try {
      const values = await formDados.validateFields();
      setSavingDados(true);
      const updatedProfile = await clienteService.updateDadosPessoais({
        nome: values.nome.trim(),
        email: values.email.trim(),
        telefone: values.telefone.trim(),
      });
      updateProfile(updatedProfile);
      setEditingDados(false);
      message.success(t('perfil.dados.updated'));
    } catch (error: any) {
      if (error?.errorFields) return;
      message.error(error.message || t('perfil.dados.updateError'));
    } finally {
      setSavingDados(false);
    }
  };

  const handleEditEndereco = () => {
    formEndereco.setFieldsValue({
      cep: formatCep(profile?.endereco?.cep),
      logradouro: normalizeText(profile?.endereco?.logradouro),
      numero: normalizeText(profile?.endereco?.numero),
      complemento: normalizeText(profile?.endereco?.complemento),
      bairro: normalizeText(profile?.endereco?.bairro),
      cidade: normalizeText(profile?.endereco?.cidade),
      uf: normalizeText(profile?.endereco?.uf).toUpperCase(),
    });
    setEditingEndereco(true);
  };

  const handleBuscarCep = async () => {
    const cep = normalizeText(formEndereco.getFieldValue('cep'));
    const digits = cep.replace(/\D/g, '');
    if (digits.length !== 8) return;

    try {
      setLoadingCep(true);
      const endereco = await viaCepService.buscarEnderecoPorCep(digits);
      if (!endereco) {
        message.warning(t('perfil.endereco.cep.notFound'));
        return;
      }

      const complementoAtual = normalizeText(formEndereco.getFieldValue('complemento'));
      formEndereco.setFieldsValue({
        cep: endereco.cep,
        logradouro: endereco.logradouro,
        complemento: complementoAtual || endereco.complemento || undefined,
        bairro: endereco.bairro,
        cidade: endereco.cidade,
        uf: endereco.uf.toUpperCase(),
      });
    } catch {
      message.error(t('perfil.endereco.cep.fetchError'));
    } finally {
      setLoadingCep(false);
    }
  };

  const handleSaveEndereco = async () => {
    try {
      const values = await formEndereco.validateFields();
      setSavingEndereco(true);
      const updatedProfile = await clienteService.updateEndereco({
        cep: normalizeText(values.cep),
        logradouro: normalizeText(values.logradouro),
        numero: normalizeText(values.numero),
        complemento: normalizeText(values.complemento),
        bairro: normalizeText(values.bairro),
        cidade: normalizeText(values.cidade),
        uf: normalizeText(values.uf).toUpperCase(),
      });
      updateProfile(updatedProfile);
      setEditingEndereco(false);
      message.success(t('perfil.endereco.updated'));
    } catch (error: any) {
      if (error?.errorFields) return;
      message.error(error.message || t('perfil.endereco.updateError'));
    } finally {
      setSavingEndereco(false);
    }
  };

  const handleAlterarSenha = async () => {
    try {
      const values = await formSenha.validateFields();
      setSavingSenha(true);
      await clienteService.alterarSenha({
        senhaAtual: values.senhaAtual,
        novaSenha: values.novaSenha,
        confirmarNovaSenha: values.confirmarNovaSenha,
      });
      setSenhaModalOpen(false);
      formSenha.resetFields();
      message.success(t('perfil.senha.updated'));
    } catch (error: any) {
      if (error?.errorFields) return;
      message.error(error.message || t('perfil.senha.updateError'));
    } finally {
      setSavingSenha(false);
    }
  };

  const handleIdiomaChange = (nextIdioma: Idioma) => {
    setIdioma(nextIdioma);
  };

  const handleTemaChange = (checked: boolean) => {
    setTema(checked ? 'dark' : 'light');
  };

  if (loading) {
    return (
      <Card>
        <Skeleton active avatar paragraph={{ rows: 8 }} />
      </Card>
    );
  }

  if (!profile) {
    return (
      <Card>
        <Text type="danger">{t('perfil.load.empty')}</Text>
      </Card>
    );
  }

  return (
    <Flex vertical gap="large" style={{ width: '100%' }}>
      <Flex vertical gap="middle">
        <Breadcrumb
          items={[
            { title: <HomeOutlined /> },
            {
              title: (
                <>
                  <UserOutlined /> <span>{t('perfil.breadcrumb')}</span>
                </>
              ),
            },
          ]}
        />

        <Flex align="center" gap="large" wrap="wrap">
          <Avatar size={72} icon={<UserOutlined />} style={{ background: '#1677ff', flexShrink: 0 }} />
          <Flex vertical gap={4}>
            <Title level={2} style={{ margin: 0 }}>
              {profile.nome}
            </Title>
            <Text type="secondary">{profile.email}</Text>
          </Flex>
        </Flex>
      </Flex>

      <SectionCard
        title={t('perfil.dados.title')}
        icon={<UserOutlined />}
        editing={editingDados}
        saving={savingDados}
        onEdit={handleEditDados}
        onSave={handleSaveDados}
        onCancel={() => setEditingDados(false)}
      >
        {editingDados ? (
          <Form form={formDados} layout="vertical" style={{ maxWidth: 680 }}>
            <Form.Item
              label={t('perfil.dados.nome.label')}
              name="nome"
              rules={[{ required: true, whitespace: true, message: t('perfil.dados.nome.required') }]}
            >
              <Input prefix={<UserOutlined />} />
            </Form.Item>

            <Flex gap="large" wrap="wrap">
              <Form.Item label={t('perfil.dados.cpf.label')} style={{ flex: '1 1 180px' }}>
                <Input prefix={<IdcardOutlined />} value={formatCPF(profile.cpf)} disabled />
              </Form.Item>

              <Form.Item
                label={t('perfil.dados.telefone.label')}
                name="telefone"
                rules={[
                  { required: true, message: t('perfil.dados.telefone.required') },
                  { pattern: PHONE_REGEX, message: t('perfil.dados.telefone.invalid') },
                ]}
                style={{ flex: '1 1 180px' }}
              >
                <Input
                  placeholder="(00) 00000-0000"
                  prefix={<PhoneOutlined />}
                  onChange={(event) => {
                    formDados.setFieldValue('telefone', formatPhone(event.target.value));
                  }}
                />
              </Form.Item>
            </Flex>

            <Form.Item
              label={t('perfil.dados.email.label')}
              name="email"
              rules={[
                { required: true, message: t('perfil.dados.email.required') },
                { type: 'email', message: t('perfil.dados.email.invalid') },
              ]}
            >
              <Input prefix={<MailOutlined />} />
            </Form.Item>
          </Form>
        ) : (
          <Flex vertical gap="middle">
            <Descriptions column={{ xs: 1, sm: 2 }} size="small">
              <Descriptions.Item label={t('perfil.dados.nome.label')}>
                <Text strong>{profile.nome}</Text>
              </Descriptions.Item>
              <Descriptions.Item label={t('perfil.dados.cpf.label')}>
                <Flex align="center" gap={6}>
                  <IdcardOutlined style={{ color: '#8c8c8c' }} />
                  <Text>{formatCPF(profile.cpf)}</Text>
                </Flex>
              </Descriptions.Item>
              <Descriptions.Item label={t('perfil.dados.telefone.label')}>
                <Flex align="center" gap={6}>
                  <PhoneOutlined style={{ color: '#8c8c8c' }} />
                  <Text>{profile.telefone ? formatPhone(profile.telefone) : '-'}</Text>
                </Flex>
              </Descriptions.Item>
              <Descriptions.Item label={t('perfil.dados.email.label')}>
                <Flex align="center" gap={6}>
                  <MailOutlined style={{ color: '#8c8c8c' }} />
                  <Text>{profile.email}</Text>
                </Flex>
              </Descriptions.Item>
            </Descriptions>

            <Divider style={{ margin: '4px 0' }} />

            <Flex align="center" justify="space-between" gap="middle" wrap="wrap">
              <Flex align="center" gap={8}>
                <LockOutlined style={{ color: '#8c8c8c' }} />
                <Text type="secondary">{t('perfil.senha.label')}</Text>
                <Text>********</Text>
              </Flex>
              <Button size="small" icon={<LockOutlined />} onClick={() => setSenhaModalOpen(true)}>
                {t('perfil.senha.change')}
              </Button>
            </Flex>
          </Flex>
        )}
      </SectionCard>

      <SectionCard
        title={t('perfil.endereco.title')}
        icon={<EnvironmentOutlined />}
        editing={editingEndereco}
        saving={savingEndereco}
        onEdit={handleEditEndereco}
        onSave={handleSaveEndereco}
        onCancel={() => setEditingEndereco(false)}
      >
        {editingEndereco ? (
          <Form form={formEndereco} layout="vertical" style={{ maxWidth: 760 }}>
            <Flex gap="large" wrap="wrap">
              <Form.Item
                label={t('perfil.endereco.cep.label')}
                name="cep"
                rules={[{ required: true, message: t('perfil.endereco.cep.required') }]}
                style={{ flex: '0 0 160px' }}
              >
                <Input
                  placeholder="00000-000"
                  suffix={loadingCep ? <Spin size="small" /> : undefined}
                  onBlur={handleBuscarCep}
                  onChange={(event) => {
                    formEndereco.setFieldValue('cep', formatCepInput(event.target.value));
                  }}
                />
              </Form.Item>
              <Form.Item
                label={t('perfil.endereco.logradouro.label')}
                name="logradouro"
                rules={[{ required: true, whitespace: true, message: t('perfil.endereco.logradouro.required') }]}
                style={{ flex: '1 1 280px' }}
              >
                <Input placeholder={t('perfil.endereco.logradouro.placeholder')} />
              </Form.Item>
              <Form.Item
                label={t('perfil.endereco.numero.label')}
                name="numero"
                rules={[{ required: true, whitespace: true, message: t('perfil.endereco.numero.required') }]}
                style={{ flex: '0 0 120px' }}
              >
                <Input placeholder={t('perfil.endereco.numero.placeholder')} />
              </Form.Item>
            </Flex>

            <Flex gap="large" wrap="wrap">
              <Form.Item
                label={t('perfil.endereco.complemento.label')}
                name="complemento"
                style={{ flex: '1 1 200px' }}
              >
                <Input placeholder={t('perfil.endereco.complemento.placeholder')} />
              </Form.Item>
              <Form.Item
                label={t('perfil.endereco.bairro.label')}
                name="bairro"
                rules={[{ required: true, whitespace: true, message: t('perfil.endereco.bairro.required') }]}
                style={{ flex: '1 1 200px' }}
              >
                <Input />
              </Form.Item>
            </Flex>

            <Flex gap="large" wrap="wrap">
              <Form.Item
                label={t('perfil.endereco.cidade.label')}
                name="cidade"
                rules={[{ required: true, whitespace: true, message: t('perfil.endereco.cidade.required') }]}
                style={{ flex: '1 1 240px' }}
              >
                <Input placeholder={t('perfil.endereco.cidade.placeholder')} />
              </Form.Item>
              <Form.Item
                label={t('perfil.endereco.uf.label')}
                name="uf"
                rules={[
                  { required: true, message: t('perfil.endereco.uf.required') },
                  { len: 2, message: t('perfil.endereco.uf.invalid') },
                ]}
                style={{ flex: '0 0 100px' }}
              >
                <Input
                  placeholder={t('perfil.endereco.uf.placeholder')}
                  maxLength={2}
                  style={{ textTransform: 'uppercase' }}
                  onChange={(event) => {
                    formEndereco.setFieldValue('uf', event.target.value.toUpperCase());
                  }}
                />
              </Form.Item>
            </Flex>
          </Form>
        ) : hasEndereco(profile.endereco) && enderecoDescription ? (
          <Flex align="flex-start" gap={8}>
            <EnvironmentOutlined style={{ color: '#8c8c8c', marginTop: 3 }} />
            <Flex vertical gap={2}>
              <Text>{enderecoDescription.linha1 || '-'}</Text>
              {enderecoDescription.complemento && <Text type="secondary">{enderecoDescription.complemento}</Text>}
              {enderecoDescription.bairro && <Text type="secondary">{enderecoDescription.bairro}</Text>}
              {enderecoDescription.cidadeUf && <Text type="secondary">{enderecoDescription.cidadeUf}</Text>}
              {enderecoDescription.cep && (
                <Text type="secondary">{t('perfil.endereco.cepDisplay', { cep: enderecoDescription.cep })}</Text>
              )}
            </Flex>
          </Flex>
        ) : (
          <Flex vertical align="flex-start" gap={8}>
            <Text type="secondary">{t('perfil.endereco.empty')}</Text>
            <Button type="dashed" icon={<PlusOutlined />} onClick={handleEditEndereco}>
              {t('perfil.endereco.create')}
            </Button>
          </Flex>
        )}
      </SectionCard>

      <Card
        title={
          <Flex align="center" gap={8}>
            <SettingOutlined />
            <span>{t('perfil.preferencias.title')}</span>
          </Flex>
        }
      >
        <Flex vertical gap="large">
          <Descriptions column={1} size="middle">
            <Descriptions.Item
              label={
                <Flex align="center" gap={8}>
                  <GlobalOutlined />
                  <span>{t('perfil.preferencias.idioma')}</span>
                </Flex>
              }
            >
              <Radio.Group
                value={configuracoes.idioma}
                onChange={(event) => handleIdiomaChange(event.target.value as Idioma)}
              >
                <Space direction="vertical">
                  <Radio value="pt-BR">{t('perfil.preferencias.idioma.ptBr')}</Radio>
                  <Radio value="en-US">{t('perfil.preferencias.idioma.enUs')}</Radio>
                </Space>
              </Radio.Group>
            </Descriptions.Item>

            <Descriptions.Item
              label={
                <Flex align="center" gap={8}>
                  <BulbOutlined />
                  <span>{t('perfil.preferencias.tema')}</span>
                </Flex>
              }
            >
              <Flex align="center" gap={12}>
                <Text>{t('perfil.preferencias.tema.claro')}</Text>
                <Switch
                  checked={configuracoes.tema === 'dark'}
                  onChange={handleTemaChange}
                />
                <Text>{t('perfil.preferencias.tema.escuro')}</Text>
              </Flex>
            </Descriptions.Item>
          </Descriptions>
          <Text type="secondary">{t('perfil.preferencias.autoSave')}</Text>
        </Flex>
      </Card>

      <Modal
        title={
          <Flex align="center" gap={8}>
            <LockOutlined />
            <span>{t('perfil.senha.change')}</span>
          </Flex>
        }
        open={senhaModalOpen}
        onCancel={() => {
          setSenhaModalOpen(false);
          formSenha.resetFields();
        }}
        onOk={handleAlterarSenha}
        confirmLoading={savingSenha}
        okText={t('perfil.senha.change')}
        cancelText={t('perfil.actions.cancel')}
        destroyOnHidden
      >
        <Form form={formSenha} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item
            label={t('perfil.senha.current.label')}
            name="senhaAtual"
            rules={[{ required: true, message: t('perfil.senha.current.required') }]}
          >
            <Input.Password />
          </Form.Item>
          <Form.Item
            label={t('perfil.senha.new.label')}
            name="novaSenha"
            rules={[
              { required: true, message: t('perfil.senha.new.required') },
              { min: 6, message: t('perfil.senha.new.min') },
            ]}
          >
            <Input.Password />
          </Form.Item>
          <Form.Item
            label={t('perfil.senha.confirm.label')}
            name="confirmarNovaSenha"
            dependencies={['novaSenha']}
            rules={[
              { required: true, message: t('perfil.senha.confirm.required') },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  if (!value || getFieldValue('novaSenha') === value) {
                    return Promise.resolve();
                  }
                  return Promise.reject(new Error(t('perfil.senha.confirm.match')));
                },
              }),
            ]}
          >
            <Input.Password />
          </Form.Item>
        </Form>
      </Modal>
    </Flex>
  );
}
