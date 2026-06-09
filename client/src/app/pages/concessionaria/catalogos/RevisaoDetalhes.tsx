import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router';
import {
  Breadcrumb,
  Typography,
  Button,
  Flex,
  Spin,
  Card,
  Descriptions,
  Tabs,
  Table,
} from 'antd';
import {
  HomeOutlined,
  ToolOutlined,
  ArrowLeftOutlined,
} from '@ant-design/icons';
import { revisaoPadraoService } from '@/app/services/revisaoPadraoService';
import { handleApiError } from '@/app/utils/errorHandler';
import { RevisaoPadraoResponse } from '@/app/models/RevisaoPadrao';
import { formatCurrency } from '@/app/utils/formatters';

const { Title } = Typography;

const formatTempo = (minutos: number) => {
  if (minutos === 0) return '-';
  const h = Math.floor(minutos / 60);
  const m = minutos % 60;
  if (h === 0) return `${m}min`;
  return m > 0 ? `${h}h ${m}min` : `${h}h`;
};

export default function RevisaoDetalhes() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [loading, setLoading] = useState(true);
  const [revisao, setRevisao] = useState<RevisaoPadraoResponse | null>(null);
  const [linhaId, setLinhaId] = useState<number | null>(null);
  const [nomeLinha, setNomeLinha] = useState<string>('');

  useEffect(() => {
    if (!id) return;

    const fetchRevisaoDetails = async () => {
      try {
        setLoading(true);
        const revData = await revisaoPadraoService.getById(Number(id));
        setRevisao(revData);
        setLinhaId(revData.linhaId);
        setNomeLinha(revData.nomeLinha);
      } catch (error) {
        handleApiError(error, 'Erro ao carregar detalhes da revisão padrão.');
      } finally {
        setLoading(false);
      }
    };

    fetchRevisaoDetails();
  }, [id]);

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%', padding: '40px' }}>
        <Spin size="large" />
      </div>
    );
  }

  if (!revisao) return null;

  const pecasVal = (revisao.pecas || []).reduce((s, p) => s + p.preco * p.quantidade, 0);
  const servsVal = (revisao.servicos || []).reduce((s, sv) => s + sv.custo, 0);
  const totalVal = pecasVal + servsVal;
  const totalTempo = (revisao.servicos || []).reduce((s, sv) => s + sv.tempoEstimado, 0);

  const pecasColumns = [
    { title: 'Peça', dataIndex: 'nome', key: 'nome' },
    { title: 'Código', dataIndex: 'codigo', key: 'codigo', width: 120 },
    { title: 'Quantidade', dataIndex: 'quantidade', key: 'quantidade', width: 120, align: 'center' as const },
    {
      title: 'Valor Unitário',
      dataIndex: 'preco',
      key: 'preco',
      width: 160,
      render: (v: number) => formatCurrency(v),
    },
    {
      title: 'Valor Total',
      key: 'total',
      width: 160,
      render: (_, record: any) => formatCurrency(record.preco * record.quantidade),
    },
  ];

  const servicosColumns = [
    { title: 'Serviço', dataIndex: 'nome', key: 'nome' },
    {
      title: 'Tempo Médio',
      dataIndex: 'tempoEstimado',
      key: 'tempoEstimado',
      width: 180,
      render: (v: number) => formatTempo(v),
    },
    {
      title: 'Valor de Mão de Obra',
      dataIndex: 'custo',
      key: 'custo',
      width: 200,
      render: (v: number) => formatCurrency(v),
    },
  ];

  const tabItems = [
    {
      key: 'pecas',
      label: `Peças (${(revisao.pecas || []).length})`,
      children: (
        <Table
          bordered
          dataSource={revisao.pecas || []}
          columns={pecasColumns}
          rowKey="id"
          pagination={false}
        />
      ),
    },
    {
      key: 'servicos',
      label: `Serviços (${(revisao.servicos || []).length})`,
      children: (
        <Table
          bordered
          dataSource={revisao.servicos || []}
          columns={servicosColumns}
          rowKey="id"
          pagination={false}
        />
      ),
    },
  ];

  return (
    <Flex vertical gap="large" style={{ width: '100%' }}>
      <Flex vertical gap="middle" style={{ width: '100%' }}>
        <Breadcrumb
          items={[
            { href: '', title: <HomeOutlined /> },
            {
              title: (
                <>
                  <ToolOutlined />
                  <span> Catálogos</span>
                </>
              ),
            },
            {
              title: 'Modelos de Revisão',
              onClick: () => navigate('/dashboard/concessionaria/catalogos-revisoes'),
              style: { cursor: 'pointer' },
            },
            {
              title: `Plano Padrão — ${nomeLinha}`,
              onClick: () => navigate(`/dashboard/concessionaria/catalogos-revisoes/linha/${linhaId}`),
              style: { cursor: 'pointer' },
            },
            { title: `Revisão ${revisao.ordem}ª` },
          ]}
        />

        <Flex align="center" gap="middle">
          <Button
            type="default"
            icon={<ArrowLeftOutlined />}
            onClick={() => navigate(`/dashboard/concessionaria/catalogos-revisoes/linha/${linhaId}`)}
          />
          <Title level={2} style={{ margin: 0 }}>
            Revisão {revisao.ordem}ª — Plano Padrão — {nomeLinha}
          </Title>
        </Flex>
      </Flex>

      <Card title="Resumo da Revisão">
        <Descriptions bordered column={2}>
          <Descriptions.Item label="Modelo de Revisão" span={2}>
            Plano Padrão — {nomeLinha}
          </Descriptions.Item>
          <Descriptions.Item label="Linha de Moto">
            {nomeLinha}
          </Descriptions.Item>
          <Descriptions.Item label="Número da Revisão">
            {revisao.ordem}ª Revisão
          </Descriptions.Item>
          <Descriptions.Item label="Quilometragem Máxima">
            {revisao.quilometragem.toLocaleString('pt-BR')} km
          </Descriptions.Item>
          <Descriptions.Item label="Tempo Máximo">
            {revisao.tempoMeses} meses
          </Descriptions.Item>
          <Descriptions.Item label="Quantidade de Peças">
            {(revisao.pecas || []).length}
          </Descriptions.Item>
          <Descriptions.Item label="Quantidade de Serviços">
            {(revisao.servicos || []).length}
          </Descriptions.Item>
          <Descriptions.Item label="Tempo Estimado">
            {formatTempo(totalTempo)}
          </Descriptions.Item>
          <Descriptions.Item label="Valor Médio">
            <strong>{formatCurrency(totalVal)}</strong>
          </Descriptions.Item>
        </Descriptions>
      </Card>

      <Card>
        <Tabs items={tabItems} />
      </Card>

      <Card title="Resumo Financeiro" style={{ background: '#f0f2f5' }}>
        <Flex vertical gap="middle" style={{ width: '100%' }}>
          <Flex justify="space-between">
            <span>Subtotal de Peças:</span>
            <strong>{formatCurrency(pecasVal)}</strong>
          </Flex>
          <Flex justify="space-between">
            <span>Subtotal de Serviços:</span>
            <strong>{formatCurrency(servsVal)}</strong>
          </Flex>
          <div style={{ borderTop: '1px solid #d9d9d9', paddingTop: '12px', marginTop: '12px' }}>
            <Flex justify="space-between" style={{ fontSize: '16px' }}>
              <span><strong>Valor Médio Total da Revisão:</strong></span>
              <strong style={{ color: '#1890ff', fontSize: '18px' }}>{formatCurrency(totalVal)}</strong>
            </Flex>
          </div>
          <Flex justify="space-between" style={{ fontSize: '16px' }}>
            <span><strong>Tempo Estimado Total:</strong></span>
            <strong>{formatTempo(totalTempo)}</strong>
          </Flex>
        </Flex>
      </Card>
    </Flex>
  );
}
