import { useEffect, useMemo, useState } from 'react';
import { useParams, useNavigate } from 'react-router';
import {
  Breadcrumb,
  Typography,
  Button,
  Table,
  Flex,
  Spin,
  Card,
  Descriptions,
} from 'antd';
import {
  HomeOutlined,
  ToolOutlined,
  ArrowLeftOutlined,
  EyeOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { revisaoPadraoService } from '@/app/services/revisaoPadraoService';
import { linhaService } from '@/app/services/linhaService';
import { modeloMotoService } from '@/app/services/modeloMotoService';
import { handleApiError } from '@/app/utils/errorHandler';
import { Linha } from '@/app/models/Linha';
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

export default function ModeloRevisaoDetalhes() {
  const { linhaId } = useParams<{ linhaId: string }>();
  const navigate = useNavigate();

  const [linha, setLinha] = useState<Linha | null>(null);
  const [revisoesDetalhes, setRevisoesDetalhes] = useState<RevisaoPadraoResponse[]>([]);
  const [modelosVinculados, setModelosVinculados] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!linhaId) return;

    const fetchPlanoDetails = async () => {
      try {
        setLoading(true);
        const lid = Number(linhaId);

        const [linhaData, lineRevisoes, allModelos] = await Promise.all([
          linhaService.getById(lid),
          revisaoPadraoService.listar({ linhaId: lid }),
          modeloMotoService.getAll(),
        ]);

        setLinha(linhaData);

        const lineModelos = allModelos.filter((m) => m.linhaId === lid);
        setModelosVinculados(lineModelos.map((m) => m.nomeModelo));

        const details = await Promise.all(
          lineRevisoes.map((r) => revisaoPadraoService.getById(r.id))
        );

        setRevisoesDetalhes(details.sort((a, b) => a.ordem - b.ordem));
      } catch (error) {
        handleApiError(error, 'Erro ao carregar detalhes do plano de revisões.');
      } finally {
        setLoading(false);
      }
    };

    fetchPlanoDetails();
  }, [linhaId]);

  const totalValor = useMemo(() => {
    return revisoesDetalhes.reduce((s, r) => {
      const pecasVal = (r.pecas || []).reduce((sum, p) => sum + p.preco * p.quantidade, 0);
      const servsVal = (r.servicos || []).reduce((sum, sv) => sum + sv.custo, 0);
      return s + pecasVal + servsVal;
    }, 0);
  }, [revisoesDetalhes]);

  const totalTempo = useMemo(() => {
    return revisoesDetalhes.reduce((s, r) => {
      return s + (r.servicos || []).reduce((sum, sv) => sum + sv.tempoEstimado, 0);
    }, 0);
  }, [revisoesDetalhes]);

  const columns: ColumnsType<RevisaoPadraoResponse> = [
    {
      title: 'Nº Revisão',
      dataIndex: 'ordem',
      key: 'ordem',
      width: 110,
      render: (n: number) => `${n}ª`,
    },
    {
      title: 'Quilometragem Máxima',
      dataIndex: 'quilometragem',
      key: 'quilometragem',
      render: (v: number) => `${v.toLocaleString('pt-BR')} km`,
    },
    {
      title: 'Tempo Máximo',
      dataIndex: 'tempoMeses',
      key: 'tempoMeses',
      render: (v: number) => `${v} meses`,
    },
    {
      title: 'Qtd. Peças',
      key: 'pecasCount',
      width: 110,
      align: 'center',
      render: (_, record) => (record.pecas || []).length,
    },
    {
      title: 'Qtd. Serviços',
      key: 'servicosCount',
      width: 120,
      align: 'center',
      render: (_, record) => (record.servicos || []).length,
    },
    {
      title: 'Tempo Estimado',
      key: 'tempoEstimado',
      width: 150,
      render: (_, record) => {
        const tempo = (record.servicos || []).reduce((s, sv) => s + sv.tempoEstimado, 0);
        return formatTempo(tempo);
      },
    },
    {
      title: 'Valor Médio',
      key: 'valorTotal',
      width: 150,
      render: (_, record) => {
        const pecasVal = (record.pecas || []).reduce((s, p) => s + p.preco * p.quantidade, 0);
        const servsVal = (record.servicos || []).reduce((s, sv) => s + sv.custo, 0);
        return formatCurrency(pecasVal + servsVal);
      },
    },
    {
      title: 'Ações',
      key: 'actions',
      width: 120,
      align: 'center',
      render: (_, record) => (
        <Button
          type="link"
          icon={<EyeOutlined />}
          onClick={() => navigate(`/dashboard/concessionaria/catalogos-revisoes/${record.id}`)}
        >
          Ver Itens
        </Button>
      ),
    },
  ];

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%', padding: '40px' }}>
        <Spin size="large" />
      </div>
    );
  }

  const nomeLinha = linha?.nome || `Linha ${linhaId}`;

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
            { title: `Plano Padrão — ${nomeLinha}` },
          ]}
        />

        <Flex align="center" gap="middle">
          <Button
            type="default"
            icon={<ArrowLeftOutlined />}
            onClick={() => navigate('/dashboard/concessionaria/catalogos-revisoes')}
          />
          <Title level={2} style={{ margin: 0 }}>
            Plano Padrão — {nomeLinha}
          </Title>
        </Flex>
      </Flex>

      <Card>
        <Descriptions bordered column={2}>
          <Descriptions.Item label="Nome do Modelo" span={2}>
            Plano Padrão — {nomeLinha}
          </Descriptions.Item>
          <Descriptions.Item label="Linha de Moto">
            {nomeLinha}
          </Descriptions.Item>
          <Descriptions.Item label="Quantidade de Revisões">
            {revisoesDetalhes.length}
          </Descriptions.Item>
          <Descriptions.Item label="Modelos de Moto Vinculados" span={2}>
            {modelosVinculados.join(', ')} ({modelosVinculados.length} modelos)
          </Descriptions.Item>
          <Descriptions.Item label="Valor Médio Total">
            {formatCurrency(totalValor)}
          </Descriptions.Item>
          <Descriptions.Item label="Tempo Estimado Total">
            {formatTempo(totalTempo)}
          </Descriptions.Item>
        </Descriptions>
      </Card>

      <Card title="Revisões do Modelo">
        <Table
          dataSource={revisoesDetalhes}
          columns={columns}
          rowKey="id"
          pagination={false}
          bordered
        />
      </Card>
    </Flex>
  );
}
