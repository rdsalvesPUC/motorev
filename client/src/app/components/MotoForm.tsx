
import { useState, useEffect } from 'react';
import {
  Typography,
  Form,
  Input,
  Select,
  Button,
  Space,
  Breadcrumb,
  Card,
  message,
  Spin,
  Upload,
} from 'antd';
import {
  HomeOutlined,
  CarOutlined,
  ArrowLeftOutlined,
  PlusOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { PATHS } from '../paths';
import { BASE_URL } from '../services/http';
import { modeloMotoService } from '../services/modeloMotoService';
import { concessionariaService } from '../services/concessionariaService';
import { motoService } from '../services/motoService';
import { ModeloMoto } from '../models/ModeloMoto';
import { Concessionaria } from '../models/Concessionaria';
import { handleApiError } from '../utils/errorHandler';
import { t } from '../i18n';

const { Title } = Typography;

export default function MotoForm() {
  const navigate = useNavigate();
  const [form] = Form.useForm();

  const [modelos, setModelos] = useState<ModeloMoto[]>([]);
  const [concessionarias, setConcessionarias] = useState<Concessionaria[]>([]);
  const [loadingModelos, setLoadingModelos] = useState<boolean>(true);
  const [loadingConcessionarias, setLoadingConcessionarias] = useState<boolean>(true);
  const [submitting, setSubmitting] = useState<boolean>(false);
  const [fotoUrl, setFotoUrl] = useState<string | undefined>(undefined);
  const [fileList, setFileList] = useState<any[]>([]);

  const customUpload = async (options: any) => {
    const { onSuccess, onError, file } = options;
    try {
      const res = await motoService.uploadImage(file as File);
      setFotoUrl(res.url);

      const backendOrigin = BASE_URL.replace(/\/api$/, '');
      const fullUrl = `${backendOrigin}${res.url}`;

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
      try {
        const modelsData = await modeloMotoService.getAll();
        setModelos(modelsData);
      } catch (error) {
        handleApiError(error, 'error.fetchModelosMotos');
      } finally {
        setLoadingModelos(false);
      }

      try {
        const concessionariasData = await concessionariaService.getAll();
        setConcessionarias(concessionariasData);
      } catch (error) {
        handleApiError(error, 'Falha ao buscar concessionárias');
      } finally {
        setLoadingConcessionarias(false);
      }
    };

    loadFormData();
  }, []);

  const onFinish = async (values: any) => {
    setSubmitting(true);
    try {
      await motoService.create({
        placa: values.placa,
        chassi: values.chassi,
        modeloMotoId: values.modeloMotoId,
        concessionariaId: values.concessionariaId,
        foto: fotoUrl,
      });
      message.success(t('moto.cadastro.success'));
      navigate(PATHS.CLIENTE_MOTOS);
    } catch (error) {
      handleApiError(error, 'error.createMoto');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <Breadcrumb
        items={[
          {
            title: (
              <span style={{ cursor: 'pointer' }} onClick={() => navigate(PATHS.DASHBOARD_CLIENTE)}>
                <HomeOutlined style={{ marginRight: 4 }} />
                Início
              </span>
            ),
          },
          {
            title: (
              <span style={{ cursor: 'pointer' }} onClick={() => navigate(PATHS.CLIENTE_MOTOS)}>
                <CarOutlined style={{ marginRight: 4 }} />
                Minhas Motos
              </span>
            ),
          },
          {
            title: t('motoForm.title'),
          },
        ]}
      />

      <Space align="center" size="middle">
        <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(PATHS.CLIENTE_MOTOS)}>
          {t('motoForm.back')}
        </Button>
        <Title level={2} style={{ margin: 0 }}>
          {t('motoForm.title')}
        </Title>
      </Space>

      <Card>
        {(loadingModelos || loadingConcessionarias) ? (
          <div style={{ display: 'flex', justifyContent: 'center', padding: '40px 0' }}>
            <Spin size="large" />
          </div>
        ) : (
          <Form form={form} layout="vertical" onFinish={onFinish}>
            <Form.Item
              name="modeloMotoId"
              label={t('motoForm.modelo.label')}
              rules={[{ required: true, message: t('motoForm.modelo.required') }]}
            >
              <Select
                placeholder={t('motoForm.modelo.placeholder')}
                options={modelos.map((m) => ({
                  value: m.id,
                  label: `${m.marca} ${m.nomeModelo}`,
                }))}
                notFoundContent={t('motoForm.emptyModelos')}
              />
            </Form.Item>

            <Form.Item
              name="concessionariaId"
              label={t('motoForm.concessionaria.label')}
            >
              <Select
                placeholder={t('motoForm.concessionaria.placeholder')}
                allowClear
                options={concessionarias.map((c) => ({
                  value: c.id,
                  label: c.nome,
                }))}
                notFoundContent={t('motoForm.emptyConcessionarias')}
              />
            </Form.Item>

            <Form.Item
              name="placa"
              label={t('motoForm.placa.label')}
              rules={[
                { required: true, message: t('motoForm.placa.required') },
                {
                  pattern: /^[a-zA-Z]{3}-?[0-9][a-zA-Z0-9][0-9]{2}$/,
                  message: t('motoForm.placa.invalid'),
                },
              ]}
            >
              <Input placeholder={t('motoForm.placa.placeholder')} style={{ textTransform: 'uppercase' }} />
            </Form.Item>

            <Form.Item
              name="chassi"
              label={t('motoForm.chassi.label')}
              rules={[
                { required: true, message: t('motoForm.chassi.required') },
                {
                  pattern: /^[a-zA-Z0-9]{17}$/,
                  message: t('motoForm.chassi.invalid'),
                },
              ]}
            >
              <Input
                maxLength={17}
                placeholder={t('motoForm.chassi.placeholder')}
                style={{ textTransform: 'uppercase' }}
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

            <Form.Item style={{ marginBottom: 0, marginTop: 24 }}>
              <Space>
                <Button onClick={() => navigate(PATHS.CLIENTE_MOTOS)}>
                  {t('motoForm.cancel')}
                </Button>
                <Button type="primary" htmlType="submit" loading={submitting}>
                  {t('motoForm.submit')}
                </Button>
              </Space>
            </Form.Item>
          </Form>
        )}
      </Card>
    </Space>
  );
}
