
import { useState, useEffect } from 'react';
import {
  Typography,
  Form,
  Input,
  Select,
  Button,
  Space,
  Card,
  message,
  Spin,
  Upload,
  InputNumber,
  DatePicker,
  Row,
  Col,
  Flex,
  Descriptions,
  Tag,
  Alert,
} from 'antd';
import {
  CarOutlined,
  ArrowLeftOutlined,
  PlusOutlined,
  InfoCircleOutlined,
  LockOutlined,
} from '@ant-design/icons';
import { useNavigate, useParams } from 'react-router';
import { PATHS, PATH_SEGMENTS } from '@/app/paths';
import dayjs from 'dayjs';
import { BASE_URL } from '@/app/services/http';
import { modeloMotoService } from '@/app/services/modeloMotoService';
import { concessionariaService } from '@/app/services/concessionariaService';
import { motoService } from '@/app/services/motoService';
import { linhaService } from '@/app/services/linhaService';
import { ModeloMoto } from '@/app/models/ModeloMoto';
import { Concessionaria } from '@/app/models/Concessionaria';
import { Linha } from '@/app/models/Linha';
import { MotoRequest } from '@/app/models/MotoRequest';
import { MotoUpdateRequest } from '@/app/models/MotoUpdateRequest';
import { handleApiError } from '@/app/utils/errorHandler';
import { t } from '@/app/i18n';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { getImageUrl } from '@/app/utils/imageUtils';

const { Title, Text } = Typography;

export default function MotoForm() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;
  const [form] = Form.useForm();

  const [modelos, setModelos] = useState<ModeloMoto[]>([]);
  const [concessionarias, setConcessionarias] = useState<Concessionaria[]>([]);
  const [linhas, setLinhas] = useState<Linha[]>([]);
  const [loadingModelos, setLoadingModelos] = useState<boolean>(true);
  const [loadingConcessionarias, setLoadingConcessionarias] = useState<boolean>(true);
  const [loadingMoto, setLoadingMoto] = useState<boolean>(isEdit);
  const [submitting, setSubmitting] = useState<boolean>(false);
  const [fotoUrl, setFotoUrl] = useState<string | undefined>(undefined);
  const [fileList, setFileList] = useState<any[]>([]);

  const [modeloSelecionado, setModeloSelecionado] = useState<ModeloMoto | null>(null);
  const [originalKm, setOriginalKm] = useState<number>(0);

  const customUpload = async (options: any) => {
    const { onSuccess, onError, file } = options;
    try {
      const res = await motoService.uploadImage(file as File);
      setFotoUrl(res.url);

      const fullUrl = getImageUrl(res.url);

      file.status = 'done';
      file.url = fullUrl;

      onSuccess(res, file);
      message.success(t('motoForm.foto.success'));
    } catch (error) {
      file.status = 'error';
      onError(error);
      handleApiError(error, t('motoForm.foto.error'));
    }
  };

  const handleUploadChange = ({ fileList: newFileList }: any) => {
    if (newFileList.length === 0) {
      setFotoUrl(undefined);
    }
    setFileList(newFileList);
  };

  useEffect(() => {
    const loadFormData = async () => {
      let modelsData: ModeloMoto[] = [];
      try {
        modelsData = await modeloMotoService.getAll();
        setModelos(modelsData);
      } catch (error) {
        handleApiError(error, 'error.fetchModelosMotos');
      } finally {
        setLoadingModelos(false);
      }

      try {
        const linesData = await linhaService.getAll(true);
        setLinhas(linesData);
      } catch (error) {
        handleApiError(error, 'error.fetchLinhas');
      }

      try {
        const concessionariasData = await concessionariaService.getAll();
        setConcessionarias(concessionariasData);
      } catch (error) {
        handleApiError(error, 'Falha ao buscar concessionárias');
      } finally {
        setLoadingConcessionarias(false);
      }

      if (isEdit) {
        try {
          const moto = await motoService.getById(Number(id));
          form.setFieldsValue({
            ...moto,
            dataVenda: moto.dataVenda ? dayjs(moto.dataVenda) : undefined,
          });
          setOriginalKm(moto.kilometragemAtual);

          // Buscar e setar o modelo selecionado para exibir as especificações
          if (modelsData.length > 0) {
            const modelo = modelsData.find(m => m.id === moto.modeloMotoId);
            if (modelo) {
              setModeloSelecionado(modelo);
            }
          }

          if (moto.foto) {
            setFotoUrl(moto.foto);
            setFileList([{
              uid: '-1',
              name: 'foto.png',
              status: 'done',
              url: getImageUrl(moto.foto),
            }]);
          }
        } catch (error) {
          handleApiError(error, t('error.getMoto'));
          navigate(PATHS.CLIENTE_MOTOS);
        } finally {
          setLoadingMoto(false);
        }
      }
    };

    loadFormData();
  }, [id, isEdit, form, navigate]);

  const getLinhaNome = (linhaId: number) => {
    return linhas.find(l => l.id === linhaId)?.nome || '';
  };

  const handleModeloChange = (id: number) => {
    const modelo = modelos.find((m) => m.id === id) ?? null;
    setModeloSelecionado(modelo);
  };

  const modeloOptions = modelos.map((m) => ({
    value: m.id,
    label: `${m.marca} ${m.nomeModelo} ${m.ano} · ${m.cilindrada} · ${getLinhaNome(m.linhaId)}`,
  }));

  const onFinish = async (values: any) => {
    setSubmitting(true);
    try {
      if (isEdit) {
        const updateData: MotoUpdateRequest = {
          placa: values.placa,
          cor: values.cor,
          kilometragemAtual: values.kilometragemAtual,
        };
        await motoService.update(Number(id), updateData);
        message.success(t('moto.atualizacao.success'));
      } else {
        const motoData: MotoRequest = {
          placa: values.placa,
          chassi: values.chassi,
          modeloMotoId: values.modeloMotoId,
          concessionariaId: values.concessionariaId,
          foto: fotoUrl,
          cor: values.cor,
          kilometragemAtual: values.kilometragemAtual,
          dataVenda: values.dataVenda.format('YYYY-MM-DD'),
        };
        await motoService.create(motoData);
        message.success(t('moto.cadastro.success'));
      }
      navigate(PATHS.CLIENTE_MOTOS);
    } catch (error) {
      handleApiError(error, isEdit ? t('error.updateMoto') : t('error.createMoto'));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Flex vertical gap="large" style={{ width: '100%' }}>
      <DashboardBreadcrumb
        userType="cliente"
        items={[
          {
            title: t('minhasMotos.title'),
            path: PATH_SEGMENTS.CLIENTE_MOTOS,
            icon: <CarOutlined />,
          },
          {
            title: isEdit ? t('motoForm.titleEdit') : t('motoForm.title'),
          },
        ]}
      />

      <Flex align="center" gap="middle">
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(PATHS.CLIENTE_MOTOS)}>
          {t('motoForm.back')}
        </Button>
        <Title level={2} style={{ margin: 0 }}>
          {isEdit ? t('motoForm.titleEdit') : t('motoForm.title')}
        </Title>
      </Flex>

      {(loadingModelos || loadingConcessionarias || loadingMoto) ? (
        <Card>
          <div style={{ display: 'flex', justifyContent: 'center', padding: '40px 0' }}>
            <Spin size="large" />
          </div>
        </Card>
      ) : (
        <Form form={form} layout="vertical" onFinish={onFinish} style={{ width: '100%' }}>
          {/* Seção 1: Modelo */}
          <Card
            title={
              <Flex align="center" gap={8}>
                <CarOutlined />
                <span>{t('motoForm.secao.modelo')}</span>
              </Flex>
            }
            style={{ marginBottom: 24 }}
          >
            {isEdit ? (
              /* No modo edição, o Modelo é exibido somente-leitura com as especificações */
              <>
                <Alert
                  type="warning"
                  icon={<LockOutlined />}
                  showIcon
                  message={t('motoForm.edit.readonlyInfo')}
                  style={{ marginBottom: 16 }}
                />
                {modeloSelecionado && (
                  <Descriptions
                    size="small"
                    bordered
                    column={{ xs: 2, sm: 4 }}
                  >
                    <Descriptions.Item label={t('motoForm.marca')}>
                      <span>{modeloSelecionado.marca}</span>
                    </Descriptions.Item>
                    <Descriptions.Item label={t('motoForm.modelo')}>
                      <span>{modeloSelecionado.nomeModelo}</span>
                    </Descriptions.Item>
                    <Descriptions.Item label={t('motoForm.ano.label')}>
                      <Tag>{modeloSelecionado.ano}</Tag>
                    </Descriptions.Item>
                    <Descriptions.Item label={t('motoForm.cilindrada.label')}>
                      <Tag color="purple">{modeloSelecionado.cilindrada}</Tag>
                    </Descriptions.Item>
                    <Descriptions.Item label={t('motoForm.linha.label')} span={4}>
                      <Tag color="blue">{getLinhaNome(modeloSelecionado.linhaId)}</Tag>
                    </Descriptions.Item>
                  </Descriptions>
                )}
              </>
            ) : (
              /* No modo criação, o Select de Modelo é editável */
              <>
                <Form.Item
                  label={t('motoForm.modelo.selecione')}
                  name="modeloMotoId"
                  rules={[{ required: true, message: t('motoForm.modelo.required') }]}
                  style={{ maxWidth: 520 }}
                >
                  <Select
                    showSearch
                    placeholder={t('motoForm.modelo.placeholder')}
                    options={modeloOptions}
                    onChange={handleModeloChange}
                    filterOption={(input, option) =>
                      (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                    }
                    notFoundContent={t('motoForm.emptyModelos')}
                  />
                </Form.Item>

                {modeloSelecionado && (
                  <Descriptions
                    size="small"
                    bordered
                    column={{ xs: 2, sm: 4 }}
                    style={{ marginTop: 4 }}
                  >
                    <Descriptions.Item label={t('motoForm.marca')}>
                      <span>{modeloSelecionado.marca}</span>
                    </Descriptions.Item>
                    <Descriptions.Item label={t('motoForm.modelo')}>
                      <span>{modeloSelecionado.nomeModelo}</span>
                    </Descriptions.Item>
                    <Descriptions.Item label={t('motoForm.ano.label')}>
                      <Tag>{modeloSelecionado.ano}</Tag>
                    </Descriptions.Item>
                    <Descriptions.Item label={t('motoForm.cilindrada.label')}>
                      <Tag color="purple">{modeloSelecionado.cilindrada}</Tag>
                    </Descriptions.Item>
                    <Descriptions.Item label={t('motoForm.linha.label')} span={4}>
                      <Tag color="blue">{getLinhaNome(modeloSelecionado.linhaId)}</Tag>
                    </Descriptions.Item>
                  </Descriptions>
                )}

                {!modeloSelecionado && (
                  <Alert
                    type="info"
                    icon={<InfoCircleOutlined />}
                    showIcon
                    message={t('motoForm.modelo.especificacoes')}
                    style={{ marginTop: 4 }}
                  />
                )}
              </>
            )}
          </Card>

          {/* Seção 2: Dados da Moto */}
          <Card
            title={t('motoForm.secao.dados')}
            style={{ marginBottom: 24 }}
          >
            <Flex gap="large" wrap="wrap">
              <Form.Item
                label={t('motoForm.placa.label')}
                name="placa"
                rules={[
                  { required: true, message: t('motoForm.placa.required') },
                  {
                    pattern: /^[a-zA-Z]{3}-?[0-9][a-zA-Z0-9][0-9]{2}$/,
                    message: t('motoForm.placa.invalid'),
                  },
                ]}
                style={{ flex: '0 0 160px' }}
              >
                <Input
                  placeholder={t('motoForm.placa.placeholder')}
                  style={{ textTransform: 'uppercase' }}
                  maxLength={8}
                />
              </Form.Item>

              <Form.Item
                label={t('motoForm.cor.label')}
                name="cor"
                rules={[{ required: true, message: t('motoForm.cor.required') }]}
                style={{ flex: '1 1 180px' }}
              >
                <Select
                  placeholder={t('motoForm.cor.placeholder')}
                  options={[
                    'Preta', 'Branca', 'Vermelha', 'Azul', 'Cinza',
                    'Prata', 'Verde', 'Amarela', 'Laranja', 'Rosa', 'Outra',
                  ].map((c) => ({ value: c, label: c }))}
                />
              </Form.Item>

              <Form.Item
                label={t('motoForm.dataVenda.label')}
                name="dataVenda"
                rules={[{ required: !isEdit, message: t('motoForm.dataVenda.required') }]}
                style={{ flex: '1 1 180px' }}
              >
                <DatePicker
                  format="DD/MM/YYYY"
                  placeholder="DD/MM/AAAA"
                  style={{ width: '100%' }}
                  disabledDate={(d) => d.isAfter(dayjs())}
                  disabled={isEdit}
                />
              </Form.Item>

              <Form.Item
                name="concessionariaId"
                label={t('motoForm.concessionaria.label')}
                style={{ flex: '1 1 250px' }}
              >
                <Select
                  placeholder={t('motoForm.concessionaria.placeholder')}
                  allowClear
                  options={concessionarias.map((c) => ({
                    value: c.id,
                    label: c.nome,
                  }))}
                  notFoundContent={t('motoForm.emptyConcessionarias')}
                  disabled={isEdit}
                />
              </Form.Item>
            </Flex>

              <Form.Item
                label={t('motoForm.chassi.label')}
                name="chassi"
                rules={[
                  { required: !isEdit, message: t('motoForm.chassi.required') },
                  {
                    pattern: /^[a-zA-Z0-9]{17}$/,
                    message: t('motoForm.chassi.invalid'),
                  },
                ]}
                style={{ maxWidth: 340 }}
              >
                <Input
                  placeholder={t('motoForm.chassi.placeholder')}
                  style={{ textTransform: 'uppercase' }}
                  maxLength={17}
                  disabled={isEdit}
                />
              </Form.Item>

              <Form.Item
                label={t('motoForm.kilometragem.label')}
                name="kilometragemAtual"
                rules={[
                  { required: true, message: t('motoForm.kilometragem.required') },
                  {
                    validator: (_, value) => {
                      if (isEdit && value !== undefined && value !== null && value < originalKm) {
                        return Promise.reject(new Error(t('motoForm.kilometragem.cannotDecrease', { current: originalKm })));
                      }
                      return Promise.resolve();
                    }
                  }
                ]}
                style={{ maxWidth: 220 }}
              >
                <InputNumber
                  placeholder={t('motoForm.kilometragem.placeholder')}
                  style={{ width: '100%' }}
                  min={isEdit ? originalKm : 0}
                  addonAfter="km"
                  formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, '.')}
                  parser={(v) => Number(v?.replace(/\./g, '') ?? 0)}
                />
              </Form.Item>

            <Form.Item
              label={t('motoForm.foto.label')}
              extra={t('motoForm.foto.hint')}
            >
              <Upload
                listType="picture-card"
                fileList={fileList}
                customRequest={customUpload}
                onChange={handleUploadChange}
                maxCount={1}
                beforeUpload={(file) => {
                  const isImage = file.type.startsWith('image/');
                  if (!isImage) {
                    message.error(t('motoForm.foto.invalidType'));
                    return Upload.LIST_IGNORE;
                  }
                  return true;
                }}
              >
                {fileList.length < 1 && (
                  <div>
                    <PlusOutlined />
                    <div style={{ marginTop: 8 }}>{t('motoForm.foto.upload')}</div>
                  </div>
                )}
              </Upload>
            </Form.Item>
          </Card>

          <Flex gap="middle">
            <Button size="large" onClick={() => navigate(PATHS.CLIENTE_MOTOS)}>
              {t('motoForm.cancel')}
            </Button>
            <Button
              type="primary"
              htmlType="submit"
              size="large"
              loading={submitting}
              disabled={!isEdit && !modeloSelecionado}
            >
              {isEdit ? t('motoForm.submitEdit') : t('motoForm.submit')}
            </Button>
          </Flex>
        </Form>
      )}
    </Flex>
  );
}
