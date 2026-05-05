import {Routes, Route, Navigate, useNavigate} from 'react-router';
import DashboardLayout from './DashboardLayout';
import {Typography} from 'antd';
import {tokenManager} from '../services/tokenManager';
import {PATHS} from '../paths';
import {t} from '../i18n';
import FormServico from "@/app/components/catalogos/FormServico";
import CatalogoServicos from "@/app/components/catalogos/CatalogoServicos";

const {Title, Paragraph} = Typography;

function DashboardHome() {
    return (
        <>
            <Title level={2}>{t('dashboard.welcome')}</Title>
            <Paragraph>
                {t('dashboard.dealershipAreaInfo')}
            </Paragraph>
        </>
    );
}

export default function DashboardConcessionaria() {
    const user = tokenManager.getUserData();
    const navigate = useNavigate();

    return (
        <DashboardLayout
            userType="concessionaria"
            userName={user?.nome || 'Concessionária'}
        >
            <Routes>
                <Route index element={<DashboardHome/>}/>
                <Route path="catalogos-servicos" element={<CatalogoServicos onNavigateToForm={() => navigate('/dashboard/concessionaria/catalogos-servicos/novo')} />} />
                <Route path="catalogos-servicos/novo" element={<FormServico onCancel={() => navigate('/dashboard/concessionaria/catalogos-servicos')} />} />
                <Route path="*" element={<Navigate to={PATHS.DASHBOARD_CONCESSIONARIA} replace/>}/>
            </Routes>
        </DashboardLayout>
    );
}
