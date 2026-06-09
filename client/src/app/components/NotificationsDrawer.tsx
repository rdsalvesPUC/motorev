import React from 'react';
import { Drawer, List, Typography, Button, Empty, Tag, Flex, Badge, Divider } from 'antd';
import {
  BellOutlined,
  CheckOutlined,
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
  const iconMap: Record<TipoAlerta, React.ReactNode> = {
    RevisaoProxima: <ClockCircleOutlined style={{ color: '#faad14' }} />,
    RevisaoAtrasada: <WarningOutlined style={{ color: '#ff4d4f' }} />,
    AgendamentoCriado: <CalendarOutlined style={{ color: '#1677ff' }} />,
    AgendamentoAlterado: <CalendarOutlined style={{ color: '#faad14' }} />,
    RevisaoConcluida: <CheckCircleOutlined style={{ color: '#52c41a' }} />,
  };
  return iconMap[tipo] || <InfoCircleOutlined />;
}

function NotificationTag({ tipo }: { tipo: TipoAlerta }) {
  const tagMap: Record<TipoAlerta, { label: string; color: string }> = {
    RevisaoProxima: { label: t('status.revisaoProxima', 'Próxima'), color: 'orange' },
    RevisaoAtrasada: { label: t('status.revisaoAtrasada', 'Atrasada'), color: 'red' },
    AgendamentoCriado: { label: t('status.novo', 'Novo'), color: 'blue' },
    AgendamentoAlterado: { label: t('status.alterado', 'Alterado'), color: 'orange' },
    RevisaoConcluida: { label: t('status.concluido', 'Concluído'), color: 'green' },
  };

  const config = tagMap[tipo];
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

  return (
    <Drawer
      title={
        <Flex justify="space-between" align="center">
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
              {t('alertas.drawer.markAllAsRead')}
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
                background: notif.lido ? '#fff' : '#f0f5ff',
                borderBottom: '1px solid #f0f0f0',
                transition: 'background 0.2s',
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.background = notif.lido ? '#fafafa' : '#e6f4ff';
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.background = notif.lido ? '#fff' : '#f0f5ff';
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
                  style={{ margin: 0, fontSize: 13, color: '#595959' }}
                  ellipsis={{ rows: 2 }}
                >
                  {t(`alertas.mensagem.${notif.tipo}`, { km: notif.quilometragem })}
                </Paragraph>

                <Flex justify="space-between" align="center">
                  <Text type="secondary" style={{ fontSize: 12 }}>
                    {new Date(notif.criadoEm).toLocaleString()}
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
                      {t('alertas.marcarLida', 'Marcar como lida')}
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
          <div style={{ padding: '12px 24px', textAlign: 'center', background: '#fafafa' }}>
            <Text type="secondary" style={{ fontSize: 12 }}>
              {t('alertas.drawer.recentOnly', 'Exibindo notificações recentes')}
            </Text>
          </div>
        </>
      )}
    </Drawer>
  );
}
