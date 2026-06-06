import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router';
import { Typography, Spin, Alert, Card, Row, Col, Button, Tag, Space } from 'antd';
import { ArrowLeftOutlined, ToolOutlined } from '@ant-design/icons';
import { servicoService } from '@/app/services/servicoService';
import { Servico } from '@/app/models/Servico';
import { t } from '@/app/i18n';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { ApiError } from '@/app/services/http';
import { PATH_SEGMENTS } from '@/app/paths';

const { Title, Text, Paragraph } = Typography;

export default function DetalheServico() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [servico, setServico] = useState<Servico | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (id) {
      const fetchServico = async () => {
        try {
          setLoading(true);
          setError(null);
          const data = await servicoService.getById(Number(id));
          setServico(data);
        } catch (err) {
          if (err instanceof ApiError) {
            if (err.status === 404) {
              setError(t('error.serviceNotFound'));
            } else {
              setError(t('error.apiError')); // Generic API error message
            }
          } else {
            setError(t('error.unexpected'));
          }
        } finally {
          setLoading(false);
        }
      };
      fetchServico();
    }
  }, [id]);

  const handleBack = () => {
    navigate(-1); // Navega para a página anterior no histórico
  };

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
        <Spin size="large" />
      </div>
    );
  }

  return (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <DashboardBreadcrumb
        userType="concessionaria"
        items={[
          { title: t('serviceCatalog.title'), path: PATH_SEGMENTS.CONCESSIONARIA_CATALOGOS_SERVICOS, icon: <ToolOutlined /> },
          { title: servico ? servico.nome : t('serviceDetail.title') },
        ]}
      />
      
      <Title level={2} style={{ margin: 0 }}>
        {servico ? servico.nome : t('serviceDetail.title')}
      </Title>

      {error && (
        <Alert 
          message={error} 
          type="error" 
          showIcon 
          action={
            <Button size="small" type="primary" onClick={handleBack}>
              {t('back')}
            </Button>
          }
        />
      )}

      {servico && (
        <Card>
          <Space direction="vertical" size="middle" style={{ width: '100%' }}>
            <Row gutter={[16, 16]} align="middle">
              <Col>
                <Button icon={<ArrowLeftOutlined />} onClick={handleBack}>
                  {t('back')}
                </Button>
              </Col>
            </Row>

            <Row gutter={[32, 32]} style={{ marginTop: '24px' }}>
              <Col xs={24} md={16}>
                <Title level={4}>{t('serviceDetail.sectionTitle')}</Title>
                <Paragraph>
                  <Text strong>{t('serviceCatalog.description')}:</Text> {servico.descricao}
                </Paragraph>
                <Paragraph>
                  <Text strong>{t('serviceCatalog.category')}:</Text> <Tag>{t(`serviceCatalog.category.${servico.categoria.toLowerCase()}`)}</Tag>
                </Paragraph>
                <Paragraph>
                  <Text strong>{t('serviceCatalog.estimatedTime')}:</Text> {servico.tempoEstimado} {t('minutes')}
                </Paragraph>
                <Paragraph>
                  <Text strong>{t('serviceCatalog.price')}:</Text> {new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(servico.custo)}
                </Paragraph>
              </Col>
            </Row>
          </Space>
        </Card>
      )}
    </Space>
  );
}
