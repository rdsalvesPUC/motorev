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
              Cancelar
            </Button>
            <Button size="small" type="primary" icon={<SaveOutlined />} onClick={onSave} loading={saving}>
              Salvar
            </Button>
          </Flex>
        ) : (
          <Button size="small" icon={<EditOutlined />} onClick={onEdit}>
            Editar
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
  const [temaEscuro, setTemaEscuro] = useState(false);
  const [idioma, setIdioma] = useState('pt-BR');

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
          message.error(error.message || 'Falha ao carregar perfil');
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
      message.success('Dados pessoais atualizados');
    } catch (error: any) {
      if (error?.errorFields) return;
      message.error(error.message || 'Falha ao atualizar dados pessoais');
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
        message.warning('CEP não encontrado');
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
    } catch (error: any) {
      message.error(error.message || 'Falha ao consultar CEP');
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
      message.success('Endereço atualizado');
    } catch (error: any) {
      if (error?.errorFields) return;
      message.error(error.message || 'Falha ao atualizar endereço');
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
      message.success('Senha alterada com sucesso');
    } catch (error: any) {
      if (error?.errorFields) return;
      message.error(error.message || 'Falha ao alterar senha');
    } finally {
      setSavingSenha(false);
    }
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
        <Text type="danger">Não foi possível carregar os dados do perfil.</Text>
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
                  <UserOutlined /> <span>Perfil</span>
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
        title="Dados Pessoais"
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
              label="Nome Completo"
              name="nome"
              rules={[{ required: true, whitespace: true, message: 'Informe o nome completo' }]}
            >
              <Input prefix={<UserOutlined />} />
            </Form.Item>

            <Flex gap="large" wrap="wrap">
              <Form.Item label="CPF" style={{ flex: '1 1 180px' }}>
                <Input prefix={<IdcardOutlined />} value={formatCPF(profile.cpf)} disabled />
              </Form.Item>

              <Form.Item
                label="Telefone"
                name="telefone"
                rules={[
                  { required: true, message: 'Informe o telefone' },
                  { pattern: PHONE_REGEX, message: 'Telefone inválido' },
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
              label="E-mail"
              name="email"
              rules={[
                { required: true, message: 'Informe o e-mail' },
                { type: 'email', message: 'E-mail inválido' },
              ]}
            >
              <Input prefix={<MailOutlined />} />
            </Form.Item>
          </Form>
        ) : (
          <Flex vertical gap="middle">
            <Descriptions column={{ xs: 1, sm: 2 }} size="small">
              <Descriptions.Item label="Nome Completo">
                <Text strong>{profile.nome}</Text>
              </Descriptions.Item>
              <Descriptions.Item label="CPF">
                <Flex align="center" gap={6}>
                  <IdcardOutlined style={{ color: '#8c8c8c' }} />
                  <Text>{formatCPF(profile.cpf)}</Text>
                </Flex>
              </Descriptions.Item>
              <Descriptions.Item label="Telefone">
                <Flex align="center" gap={6}>
                  <PhoneOutlined style={{ color: '#8c8c8c' }} />
                  <Text>{profile.telefone ? formatPhone(profile.telefone) : '-'}</Text>
                </Flex>
              </Descriptions.Item>
              <Descriptions.Item label="E-mail">
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
                <Text type="secondary">Senha</Text>
                <Text>********</Text>
              </Flex>
              <Button size="small" icon={<LockOutlined />} onClick={() => setSenhaModalOpen(true)}>
                Alterar Senha
              </Button>
            </Flex>
          </Flex>
        )}
      </SectionCard>

      <SectionCard
        title="Endereço"
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
                label="CEP"
                name="cep"
                rules={[{ required: true, message: 'Informe o CEP' }]}
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
                label="Rua / Avenida"
                name="logradouro"
                rules={[{ required: true, whitespace: true, message: 'Informe o logradouro' }]}
                style={{ flex: '1 1 280px' }}
              >
                <Input placeholder="Ex: Rua das Flores" />
              </Form.Item>
              <Form.Item
                label="Número"
                name="numero"
                rules={[{ required: true, whitespace: true, message: 'Informe o número' }]}
                style={{ flex: '0 0 120px' }}
              >
                <Input placeholder="Ex: 100" />
              </Form.Item>
            </Flex>

            <Flex gap="large" wrap="wrap">
              <Form.Item label="Complemento" name="complemento" style={{ flex: '1 1 200px' }}>
                <Input placeholder="Ex: Apto 52, Bloco B" />
              </Form.Item>
              <Form.Item
                label="Bairro"
                name="bairro"
                rules={[{ required: true, whitespace: true, message: 'Informe o bairro' }]}
                style={{ flex: '1 1 200px' }}
              >
                <Input />
              </Form.Item>
            </Flex>

            <Flex gap="large" wrap="wrap">
              <Form.Item
                label="Cidade"
                name="cidade"
                rules={[{ required: true, whitespace: true, message: 'Informe a cidade' }]}
                style={{ flex: '1 1 240px' }}
              >
                <Input placeholder="Ex: São Paulo" />
              </Form.Item>
              <Form.Item
                label="UF"
                name="uf"
                rules={[
                  { required: true, message: 'Informe a UF' },
                  { len: 2, message: 'Use a sigla com 2 letras' },
                ]}
                style={{ flex: '0 0 100px' }}
              >
                <Input
                  placeholder="Ex: SP"
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
              {enderecoDescription.cep && <Text type="secondary">CEP {enderecoDescription.cep}</Text>}
            </Flex>
          </Flex>
        ) : (
          <Flex vertical align="flex-start" gap={8}>
            <Text type="secondary">Nenhum endereço cadastrado ainda.</Text>
            <Button type="dashed" icon={<PlusOutlined />} onClick={handleEditEndereco}>
              Cadastrar Endereço
            </Button>
          </Flex>
        )}
      </SectionCard>

      <Card
        title={
          <Flex align="center" gap={8}>
            <SettingOutlined />
            <span>Preferências</span>
          </Flex>
        }
      >
        <Flex vertical gap="large">
          <Descriptions column={1} size="middle">
            <Descriptions.Item
              label={
                <Flex align="center" gap={8}>
                  <GlobalOutlined />
                  <span>Idioma</span>
                </Flex>
              }
            >
              <Radio.Group value={idioma} onChange={(event) => setIdioma(event.target.value)}>
                <Space direction="vertical">
                  <Radio value="pt-BR">Português (Brasil)</Radio>
                  <Radio value="en-US">English (United States)</Radio>
                </Space>
              </Radio.Group>
            </Descriptions.Item>

            <Descriptions.Item
              label={
                <Flex align="center" gap={8}>
                  <BulbOutlined />
                  <span>Tema</span>
                </Flex>
              }
            >
              <Flex align="center" gap={12}>
                <Text>Claro</Text>
                <Switch checked={temaEscuro} onChange={setTemaEscuro} />
                <Text>Escuro</Text>
              </Flex>
            </Descriptions.Item>
          </Descriptions>
          <Text type="secondary">
            Preferências mantidas apenas no Front por enquanto. O backend será integrado em épico próprio.
          </Text>
        </Flex>
      </Card>

      <Modal
        title={
          <Flex align="center" gap={8}>
            <LockOutlined />
            <span>Alterar Senha</span>
          </Flex>
        }
        open={senhaModalOpen}
        onCancel={() => {
          setSenhaModalOpen(false);
          formSenha.resetFields();
        }}
        onOk={handleAlterarSenha}
        confirmLoading={savingSenha}
        okText="Alterar Senha"
        cancelText="Cancelar"
        destroyOnHidden
      >
        <Form form={formSenha} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item
            label="Senha Atual"
            name="senhaAtual"
            rules={[{ required: true, message: 'Informe a senha atual' }]}
          >
            <Input.Password />
          </Form.Item>
          <Form.Item
            label="Nova Senha"
            name="novaSenha"
            rules={[
              { required: true, message: 'Informe a nova senha' },
              { min: 6, message: 'A senha deve ter no mínimo 6 caracteres' },
            ]}
          >
            <Input.Password />
          </Form.Item>
          <Form.Item
            label="Confirmar Nova Senha"
            name="confirmarNovaSenha"
            dependencies={['novaSenha']}
            rules={[
              { required: true, message: 'Confirme a nova senha' },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  if (!value || getFieldValue('novaSenha') === value) {
                    return Promise.resolve();
                  }
                  return Promise.reject(new Error('As senhas não coincidem'));
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
