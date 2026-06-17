import React from 'react';
import { Drawer, List, Typography, Button, Empty, Tag, Flex, Badge, Divider, theme } from 'antd';
import {
  BellOutlined,
  CheckOutlined,
  CloseOutlined,
  ClockCircleOutlined,
  CalendarOutlined,
  ToolOutlined,
  WarningOutlined,
  CheckCircleOutlined,
  InfoCircleOutlined,
} from '@ant-design/icons';
import { t } from '../i18n';
import { AlertaResponse, TipoAlerta } from '../models/Alerta';

const { Title, Text, Paragraph } = Typography;

interface NotificationsDrawerProps {
  open: boolean;
  onClose: () => void;
  notificacoes: AlertaResponse[];
  unreadCount: number;
  onMarkAsRead: (id: number) => void;
  onMarkAllAsRead: () => void;
}

function NotificationIcon({ tipo }: { tipo: TipoAlerta }) {
  const { token } = theme.useToken();
  const iconMap: Record<TipoAlerta, React.ReactNode> = {
    RevisaoProxima: <CalendarOutlined style={{ color: token.colorPrimary }} />,
    RevisaoAtrasada: <WarningOutlined style={{ color: token.colorError }} />,
    AgendamentoCriado: <CalendarOutlined style={{ color: token.colorPrimary }} />,
    AgendamentoAlterado: <CalendarOutlined style={{ color: token.colorWarning }} />,
    RevisaoConcluida: <CheckCircleOutlined style={{ color: token.colorSuccess }} />,
    AgendamentoAprovado: <CheckCircleOutlined style={{ color: token.colorSuccess }} />,
    AgendamentoRecusado: <CloseOutlined style={{ color: token.colorError }} />,
    NovaSolicitacao: <BellOutlined style={{ color: token.colorPrimary }} />,
    Cancelamento: <CloseOutlined style={{ color: token.colorError }} />,
    Reagendamento: <CalendarOutlined style={{ color: token.colorWarning }} />,
  };
  return iconMap[tipo] || <InfoCircleOutlined />;
}

function NotificationTag({ tipo }: { tipo: TipoAlerta }) {
  const tagMap: Record<TipoAlerta, { label: string; color: string }> = {
    RevisaoProxima: { label: t('status.revisaoProxima'), color: 'blue' },
    RevisaoAtrasada: { label: t('status.urgente'), color: 'red' },
    AgendamentoCriado: { label: t('status.novo'), color: 'blue' },
    AgendamentoAlterado: { label: t('status.reagendamento'), color: 'orange' },
    RevisaoConcluida: { label: t('status.concluido'), color: 'green' },
    AgendamentoAprovado: { label: t('status.aprovado'), color: 'green' },
    AgendamentoRecusado: { label: t('status.recusado'), color: 'red' },
    NovaSolicitacao: { label: t('status.novo'), color: 'blue' },
    Cancelamento: { label: t('status.cancelado'), color: 'red' },
    Reagendamento: { label: t('status.reagendamento'), color: 'orange' },
  };

  const config = tagMap[tipo];
  if (!config) return null;
  return <Tag color={config.color} style={{ fontSize: 11 }}>{config.label}</Tag>;
}

export default function NotificationsDrawer({
  open,
  onClose,
  notificacoes,
  unreadCount,
  onMarkAsRead,
  onMarkAllAsRead
}: NotificationsDrawerProps) {
  const { token } = theme.useToken();

  console.log('[NotificationsDrawer] Renderizando notificações:', notificacoes);

  return (
    <Drawer
      title={
        <Flex justify="space-between" align="center" style={{ width: '100%' }}>
          <Flex align="center" gap={8}>
            <Title level={4} style={{ margin: 0 }}>
              {t('alertas.drawer.title')}
            </Title>
            {unreadCount > 0 && (
              <Badge count={unreadCount} style={{ backgroundColor: '#1677ff' }} />
            )}
          </Flex>
          {unreadCount > 0 && (
            <Button
              size="small"
              onClick={onMarkAllAsRead}
              icon={<CheckOutlined />}
            >
              {t('alertas.drawer.markAll')}
            </Button>
          )}
        </Flex>
      }
      placement="right"
      onClose={onClose}
      open={open}
      width={420}
      styles={{ body: { padding: 0 } }}
    >
      {(!notificacoes || notificacoes.length === 0) ? (
        <div style={{ padding: '40px 24px' }}>
          <Empty
            description={t('alertas.drawer.empty')}
            image={Empty.PRESENTED_IMAGE_SIMPLE}
          />
        </div>
      ) : (
        <List
          dataSource={notificacoes || []}
          renderItem={(notif) => (
            <List.Item
              key={notif.id}
              style={{
                padding: '16px 24px',
                cursor: 'pointer',
                background: notif.lido ? token.colorBgContainer : token.colorPrimaryBg,
                borderBottom: `1px solid ${token.colorBorderSecondary}`,
                transition: 'background 0.2s',
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.background = notif.lido ? token.colorBgTextHover : token.colorPrimaryBgHover;
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.background = notif.lido ? token.colorBgContainer : token.colorPrimaryBg;
              }}
              onClick={() => !notif.lido && onMarkAsRead(notif.id)}
            >
              <Flex vertical gap={8} style={{ width: '100%' }}>
                <Flex justify="space-between" align="flex-start">
                  <Flex align="center" gap={8}>
                    <NotificationIcon tipo={notif.tipo} />
                    <Text strong style={{ fontSize: 14 }}>
                      {t(`alertas.titulo.${notif.tipo}`)}
                    </Text>
                  </Flex>
                  <NotificationTag tipo={notif.tipo} />
                </Flex>

                <Paragraph
                  style={{ margin: 0, fontSize: 13, color: token.colorTextDescription }}
                  ellipsis={{ rows: 2 }}
                >
                  {t(`alertas.mensagem.${notif.tipo}`, { 
                    quilometragem: notif.quilometragem,
                    ordemRevisao: notif.ordemRevisao,
                    modeloMotoNome: notif.modeloMotoNome,
                    marcaMoto: notif.marcaMoto
                  })}
                </Paragraph>

                <Flex justify="space-between" align="center">
                  <Text type="secondary" style={{ fontSize: 12 }}>
                    {new Date(notif.criadoEm).toLocaleString([], { dateStyle: 'short', timeStyle: 'short' })}
                  </Text>
                  {!notif.lido && (
                    <Button
                      type="link"
                      size="small"
                      onClick={(e) => {
                        e.stopPropagation();
                        onMarkAsRead(notif.id);
                      }}
                      icon={<CheckOutlined />}
                      style={{ padding: 0, height: 'auto', fontSize: 12 }}
                    >
                      {t('alertas.marcarLida')}
                    </Button>
                  )}
                </Flex>
              </Flex>
            </List.Item>
          )}
        />
      )}

      {notificacoes && notificacoes.length > 0 && (
        <>
          <Divider style={{ margin: 0 }} />
          <div style={{ padding: '12px 24px', textAlign: 'center', background: token.colorFillAlter }}>
            <Text type="secondary" style={{ fontSize: 12 }}>
              {t('alertas.drawer.recentOnly', 'Exibindo notificações recentes')}
            </Text>
          </div>
        </>
      )}
    </Drawer>
  );
}
