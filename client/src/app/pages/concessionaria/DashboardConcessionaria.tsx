import {Routes, Route, Navigate, useNavigate} from 'react-router';
import DashboardLayout from '@/app/components/layout/DashboardLayout';
import {Typography} from 'antd';
import {tokenManager} from '@/app/services/tokenManager';
import {PATHS, PATH_SEGMENTS} from '@/app/paths';
import {t} from '@/app/i18n';
import FormServico from "@/app/pages/concessionaria/catalogos/FormServico";
import CatalogoServicos from "@/app/pages/concessionaria/catalogos/CatalogoServicos";
import DetalheServico from '@/app/pages/concessionaria/catalogos/DetalheServico';
import CatalogoLinhas from "@/app/pages/concessionaria/catalogos/CatalogoLinhas";
import FormLinha from "@/app/pages/concessionaria/catalogos/FormLinha";
import CatalogoPecas from "@/app/pages/concessionaria/catalogos/CatalogoPecas";
import CatalogoPecasCreate from "@/app/pages/concessionaria/catalogos/CatalogoPecasCreate";
import Lojas from "@/app/pages/concessionaria/lojas/Lojas";
import FormLojas from "@/app/pages/concessionaria/lojas/FormLojas";
import PerfilConcessionaria from "@/app/pages/concessionaria/perfil/perfil";
import CatalogoMotos from "@/app/pages/concessionaria/catalogos/CatalogoMotos";
import FormCatalogoModeloMoto from "@/app/pages/concessionaria/catalogos/FormCatalogoModeloMoto";
import CatalogoRevisoes from "@/app/pages/concessionaria/catalogos/CatalogoRevisoes";
import FormCatalogoRevisoes from "@/app/pages/concessionaria/catalogos/FormCatalogoRevisoes";

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
                <Route path={PATH_SEGMENTS.PERFIL_USUARIO} element={<PerfilConcessionaria />} />
                <Route path={PATH_SEGMENTS.CONCESSIONARIA_LOJAS} element={<Lojas onNavigateToForm={() => navigate(PATHS.CONCESSIONARIA_LOJAS_NOVO)} onNavigateToEdit={(id) => navigate(`${PATHS.CONCESSIONARIA_LOJAS_EDITAR}/${id}`)} />} />
                <Route path={PATH_SEGMENTS.CONCESSIONARIA_LOJAS_NOVO} element={<FormLojas onBack={() => navigate(PATHS.CONCESSIONARIA_LOJAS)} />} />
                <Route path={`${PATH_SEGMENTS.CONCESSIONARIA_LOJAS_EDITAR}/:id`} element={<FormLojas onBack={() => navigate(PATHS.CONCESSIONARIA_LOJAS)} />} />
                <Route path={PATH_SEGMENTS.CONCESSIONARIA_CATALOGOS_PECAS} element={<CatalogoPecas />} />
                <Route path={PATH_SEGMENTS.CONCESSIONARIA_CATALOGOS_PECAS_CREATE} element={<CatalogoPecasCreate />} />
                <Route path={PATH_SEGMENTS.CONCESSIONARIA_CATALOGOS_MOTOS} element={<CatalogoMotos onNavigateToForm={() => navigate(PATHS.CONCESSIONARIA_CATALOGOS_MOTOS_NOVO)} />} />
                <Route path={PATH_SEGMENTS.CONCESSIONARIA_CATALOGOS_MOTOS_NOVO} element={<FormCatalogoModeloMoto onBack={() => navigate(PATHS.CONCESSIONARIA_CATALOGOS_MOTOS)} />} />
                <Route path={PATH_SEGMENTS.CONCESSIONARIA_CATALOGOS_REVISOES} element={<CatalogoRevisoes onNavigateToForm={() => navigate(PATHS.CONCESSIONARIA_CATALOGOS_REVISOES_NOVO)} />} />
                <Route path={PATH_SEGMENTS.CONCESSIONARIA_CATALOGOS_REVISOES_NOVO} element={<FormCatalogoRevisoes onBack={() => navigate(PATHS.CONCESSIONARIA_CATALOGOS_REVISOES)} />} />
                <Route path={PATH_SEGMENTS.CONCESSIONARIA_CATALOGOS_SERVICOS} element={<CatalogoServicos onNavigateToForm={() => navigate(PATHS.CONCESSIONARIA_CATALOGOS_SERVICOS_NOVO)} />} />
                <Route path={PATH_SEGMENTS.CONCESSIONARIA_CATALOGOS_SERVICOS_NOVO} element={<FormServico onCancel={() => navigate(PATHS.CONCESSIONARIA_CATALOGOS_SERVICOS)} />} />
                <Route path={`${PATH_SEGMENTS.CONCESSIONARIA_CATALOGOS_SERVICOS}/:id`} element={<DetalheServico />} />
                <Route path={PATH_SEGMENTS.CONCESSIONARIA_CATALOGOS_LINHAS} element={<CatalogoLinhas onNavigateToForm={() => navigate(PATHS.CONCESSIONARIA_CATALOGOS_LINHAS_NOVO)} />} />
                <Route path={PATH_SEGMENTS.CONCESSIONARIA_CATALOGOS_LINHAS_NOVO} element={<FormLinha onCancel={() => navigate(PATHS.CONCESSIONARIA_CATALOGOS_LINHAS)} />} />
                <Route path="*" element={<Navigate to={PATHS.DASHBOARD_CONCESSIONARIA} replace/>}/>
            </Routes>
        </DashboardLayout>
    );
}
