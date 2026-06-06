import svgPaths from "./svg-0yu1p4iqz5";
import imgAMechanicExaminingAMotorcycleRepairScheduleOnATabletInAWellLitGarageWithVariousToolsAndMotorcycleParts from "figma:asset/ee0577ccd06ebe5fecde033d707c8b5883f38859.png";
import imgAMotorcycleOwnerInspectingTheirMotorcycleInABrightGarageSurroundedByToolsAndEquipment from "figma:asset/acf7bfd1a7cbe4b4059b85955485e61f1a2472c1.png";
import imgAMechanicWearingABlueJumpsuitInspectingAMotorcycleInAWellLitWorkshopSurroundedByToolsAndEquipment from "figma:asset/a601213551dc6203437bcec0d305dc728effc72d.png";
import { imgContent } from "./svg-ur0rn";
import { Button } from 'antd';
import { useNavigate } from 'react-router';
import { PATHS } from '@/app/paths';

function Logo() {
  return (
    <div className="content-stretch flex h-[32px] items-center relative shrink-0" data-name="logo">
      <p className="font-['Racing_Sans_One:Regular',sans-serif] leading-none lowercase not-italic relative shrink-0 text-[#1b2128] text-[28px] whitespace-nowrap">MotoRev</p>
    </div>
  );
}

function ChevronDown() {
  return (
    <div className="relative shrink-0 size-[16px]" data-name="chevron-down">
      <svg className="absolute block inset-0 size-full" fill="none" preserveAspectRatio="none" viewBox="0 0 16 16">
        <g id="chevron-down">
          <path d="M4 6L8 10L12 6" id="Icon" stroke="var(--stroke-0, #1B2128)" strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.5" />
        </g>
      </svg>
    </div>
  );
}

function PageLink() {
  return (
    <div className="content-stretch flex gap-[4px] items-center relative shrink-0" data-name="pageLink1">
      <p className="font-['Inter:Medium',sans-serif] font-medium leading-[20px] not-italic relative shrink-0 text-[#1b2128] text-[15px] whitespace-nowrap">Início</p>
      <ChevronDown />
    </div>
  );
}

function ChevronDown1() {
  return (
    <div className="relative shrink-0 size-[16px]" data-name="chevron-down">
      <svg className="absolute block inset-0 size-full" fill="none" preserveAspectRatio="none" viewBox="0 0 16 16">
        <g id="chevron-down">
          <path d="M4 6L8 10L12 6" id="Icon" stroke="var(--stroke-0, #1B2128)" strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.5" />
        </g>
      </svg>
    </div>
  );
}

function PageLink1() {
  return (
    <div className="content-stretch flex gap-[4px] items-center relative shrink-0" data-name="pageLink2">
      <p className="font-['Inter:Medium',sans-serif] font-medium leading-[20px] not-italic relative shrink-0 text-[#1b2128] text-[15px] whitespace-nowrap">Funcionalidades</p>
      <ChevronDown1 />
    </div>
  );
}

function ChevronDown2() {
  return (
    <div className="relative shrink-0 size-[16px]" data-name="chevron-down">
      <svg className="absolute block inset-0 size-full" fill="none" preserveAspectRatio="none" viewBox="0 0 16 16">
        <g id="chevron-down">
          <path d="M4 6L8 10L12 6" id="Icon" stroke="var(--stroke-0, #1B2128)" strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.5" />
        </g>
      </svg>
    </div>
  );
}

function PageLink2() {
  return (
    <div className="content-stretch flex gap-[4px] items-center relative shrink-0" data-name="pageLink3">
      <p className="font-['Inter:Medium',sans-serif] font-medium leading-[20px] not-italic relative shrink-0 text-[#1b2128] text-[15px] whitespace-nowrap">Preços</p>
      <ChevronDown2 />
    </div>
  );
}

function ChevronDown3() {
  return (
    <div className="relative shrink-0 size-[16px]" data-name="chevron-down">
      <svg className="absolute block inset-0 size-full" fill="none" preserveAspectRatio="none" viewBox="0 0 16 16">
        <g id="chevron-down">
          <path d="M4 6L8 10L12 6" id="Icon" stroke="var(--stroke-0, #1B2128)" strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.5" />
        </g>
      </svg>
    </div>
  );
}

function PageLink3() {
  return (
    <div className="content-stretch flex gap-[4px] items-center relative shrink-0" data-name="pageLink4">
      <p className="font-['Inter:Medium',sans-serif] font-medium leading-[20px] not-italic relative shrink-0 text-[#1b2128] text-[15px] whitespace-nowrap">Contato</p>
      <ChevronDown3 />
    </div>
  );
}

function PageLinks() {
  return (
    <div className="content-stretch flex gap-[32px] items-start relative shrink-0" data-name="pageLinks">
      <PageLink />
      <PageLink1 />
      <PageLink2 />
      <PageLink3 />
    </div>
  );
}

function Div() {
  return (
    <div className="content-stretch flex gap-[48px] items-center relative shrink-0" data-name="Div">
      <Logo />
      <PageLinks />
    </div>
  );
}

function ButtonGroup() {
  const navigate = useNavigate();

  return (
    <div className="content-stretch flex gap-[16px] items-center relative shrink-0" data-name="buttonGroup">
      <Button type="default" size="middle" onClick={() => navigate(PATHS.LOGIN)}>Entrar</Button>
      <Button type="primary" size="middle" onClick={() => navigate(PATHS.CADASTRO)}>Cadastrar</Button>
    </div>
  );
}

function SoftwareCompanyHeader() {
  return (
    <div className="bg-[#fafdff] content-stretch flex items-center justify-center py-[16px] relative shrink-0 w-full z-[3]" data-name="Software Company Header">
      <div className="flex items-center justify-between w-full max-w-[1440px] px-[48px]">
        <Div />
        <ButtonGroup />
      </div>
      <div className="absolute bottom-0 h-0 left-0 right-0" data-name="divider">
        <svg className="absolute block inset-0 size-full" fill="none" preserveAspectRatio="none" viewBox="0 0 32 32">
          <g id="divider" />
        </svg>
      </div>
    </div>
  );
}

function ButtonMarketing() {
  const navigate = useNavigate();

  return (
    <div className="content-stretch flex items-center justify-center relative shrink-0" data-name="buttonMarketing">
      <Button type="primary" size="large" style={{ height: '52px', fontSize: '20px', padding: '0 32px' }} onClick={() => navigate(PATHS.CADASTRO)}>Comece Agora</Button>
    </div>
  );
}

function Content() {
  return (
    <div className="content-stretch flex flex-col gap-[96px] items-center relative shrink-0 w-full" data-name="content">
      <p className="font-['Public_Sans:Bold',sans-serif] font-bold leading-[120px] min-w-full relative shrink-0 text-[#1b2028] text-[120px] text-center tracking-[-2.4px] w-[min-content]">Revolucione a maneira como você gerencia suas revisões de moto</p>
      <ButtonMarketing />
    </div>
  );
}

function Container1() {
  return (
    <div className="absolute content-stretch flex flex-col gap-[128px] h-[669px] items-center justify-center left-0 pt-[96px] px-[48px] top-[-0.25px] w-full" style={{ backgroundImage: "url('data:image/svg+xml;utf8,<svg viewBox=\\'0 0 1440 669\\' xmlns=\\'http://www.w3.org/2000/svg\\' preserveAspectRatio=\\'none\\'><rect x=\\'0\\' y=\\'0\\' height=\\'100%\\' width=\\'100%\\' fill=\\'url(%23grad)\\' opacity=\\'0.699999988079071\\'/><defs><radialGradient id=\\'grad\\' gradientUnits=\\'userSpaceOnUse\\' cx=\\'0\\' cy=\\'0\\' r=\\'10\\' gradientTransform=\\'matrix(0.0000096227 104.28 -278.41 0.00000869 720 -10.359)\\'><stop stop-color=\\'rgba(250,252,255,1)\\' offset=\\'0.31174\\'/><stop stop-color=\\'rgba(187,207,235,0.79)\\' offset=\\'0.48381\\'/><stop stop-color=\\'rgba(125,162,214,0.58)\\' offset=\\'0.65587\\'/><stop stop-color=\\'rgba(62,117,194,0.37)\\' offset=\\'0.82794\\'/><stop stop-color=\\'rgba(0,72,173,0.16)\\' offset=\\'1\\'/></radialGradient></defs></svg>'), linear-gradient(90deg, rgb(250, 252, 255) 0%, rgb(250, 252, 255) 100%)" }} data-name="container">
      <div className="w-full max-w-[1440px]">
        <Content />
      </div>
    </div>
  );
}

function LandingPageHeroWithTaglineAndDesktopAppMockup() {
  return (
    <div className="bg-[#fafcff] h-[669px] relative shrink-0 w-full z-[3] flex items-center justify-center" data-name="Landing Page Hero With Tagline and Desktop App Mockup">
      <Container1 />
    </div>
  );
}

function TextContent() {
  return (
    <div className="relative shrink-0 w-full" data-name="textContent">
      <div className="content-stretch flex flex-col gap-[10px] items-start px-[40px] relative size-full text-[#1b2028]">
        <p className="font-['Public_Sans:Bold',sans-serif] font-bold leading-[32px] relative shrink-0 text-[28px] tracking-[-0.56px] w-full">Agendamento de Revisões</p>
        <p className="font-['Public_Sans:Regular',sans-serif] font-normal leading-[24px] relative shrink-0 text-[20px] tracking-[-0.1px] w-full">Planeje suas revisões de forma prática e rápida.</p>
      </div>
    </div>
  );
}

function AMechanicExaminingAMotorcycleRepairScheduleOnATabletInAWellLitGarageWithVariousToolsAndMotorcycleParts() {
  return (
    <div className="pointer-events-none relative rounded-[13.192px] shrink-0 size-[126.647px]" data-name="A mechanic examining a motorcycle repair schedule on a tablet in a well-lit garage with various tools and motorcycle parts.">
      <img alt="" className="absolute inset-0 max-w-none object-cover rounded-[13.192px] size-full" src={imgAMechanicExaminingAMotorcycleRepairScheduleOnATabletInAWellLitGarageWithVariousToolsAndMotorcycleParts} />
      <div aria-hidden="true" className="absolute border-[1.979px] border-[rgba(0,0,0,0)] border-solid inset-0 rounded-[13.192px]" />
    </div>
  );
}

function TextContainer() {
  return (
    <div className="content-stretch flex flex-col gap-[26.385px] items-start relative shrink-0 w-[397.091px]" data-name="Text Container">
      <div className="flex flex-col font-['Public_Sans:Bold',sans-serif] font-bold justify-center leading-[0] overflow-hidden relative shrink-0 text-[#1b2028] text-[34px] text-ellipsis tracking-[-0.68px] w-full whitespace-nowrap">
        <p className="leading-[40px] overflow-hidden text-ellipsis">Agendamento</p>
      </div>
      <div className="bg-[rgba(27,32,40,0.2)] h-[42.216px] opacity-50 rounded-[8px] shrink-0 w-full" data-name="Placeholder" />
    </div>
  );
}

function Container2() {
  return (
    <div className="-translate-x-1/2 absolute left-1/2 rounded-[29.023px] top-[135px]" data-name="Container">
      <div aria-hidden="true" className="absolute inset-0 pointer-events-none rounded-[29.023px]">
        <div className="absolute bg-white inset-0 rounded-[29.023px]" />
        <div className="absolute inset-0 mix-blend-luminosity rounded-[29.023px]" style={{ backgroundImage: "linear-gradient(166.994deg, rgba(250, 252, 255, 0) 1.2433%, rgba(0, 84, 173, 0.05) 95.008%)" }} />
      </div>
      <div className="content-stretch flex gap-[26.385px] items-center overflow-clip p-[36.939px] relative rounded-[inherit] size-full">
        <AMechanicExaminingAMotorcycleRepairScheduleOnATabletInAWellLitGarageWithVariousToolsAndMotorcycleParts />
        <TextContainer />
      </div>
      <div aria-hidden="true" className="absolute border-[1.319px] border-[rgba(73,89,110,0.2)] border-solid inset-0 pointer-events-none rounded-[29.023px] shadow-[19.789px_168.863px_47.493px_0px_rgba(0,0,0,0),13.192px_108.178px_43.535px_0px_rgba(0,0,0,0.01),6.596px_60.685px_36.939px_0px_rgba(0,0,0,0.04),2.638px_26.385px_27.704px_0px_rgba(0,0,0,0.06),1.319px_6.596px_14.512px_0px_rgba(0,0,0,0.08)]" />
    </div>
  );
}

function UiSnippet() {
  return (
    <div className="h-[405px] relative shrink-0 w-full" data-name="ui snippet">
      <Container2 />
    </div>
  );
}

function ImageDiv() {
  return (
    <div className="content-stretch flex flex-[1_0_0] flex-col items-start min-h-px relative w-full" data-name="Image div">
      <UiSnippet />
    </div>
  );
}

function Bento() {
  return (
    <div className="bg-[rgba(37,74,126,0.09)] flex-[1_0_0] h-[480px] min-w-px relative rounded-[16px]" data-name="bento1">
      <div className="content-stretch flex flex-col items-end overflow-clip pt-[40px] relative rounded-[inherit] size-full">
        <TextContent />
        <ImageDiv />
      </div>
      <div aria-hidden="true" className="absolute border-[1.5px] border-[rgba(0,0,0,0)] border-solid inset-0 pointer-events-none rounded-[16px]" />
    </div>
  );
}

function TextContent1() {
  return (
    <div className="relative shrink-0 w-full" data-name="textContent">
      <div className="content-stretch flex flex-col gap-[10px] items-start px-[40px] relative size-full text-[#1b2028]">
        <p className="font-['Public_Sans:Bold',sans-serif] font-bold leading-[32px] relative shrink-0 text-[28px] tracking-[-0.56px] w-full">Histórico da Moto</p>
        <p className="font-['Public_Sans:Regular',sans-serif] font-normal leading-[24px] relative shrink-0 text-[20px] tracking-[-0.1px] w-full">Acompanhe o histórico completo das revisões.</p>
      </div>
    </div>
  );
}

function Container3() {
  return (
    <div className="-translate-x-1/2 absolute h-[404px] left-1/2 rounded-[16px] top-[33px] w-[296px]" data-name="Container">
      <div aria-hidden="true" className="absolute inset-0 pointer-events-none rounded-[16px]">
        <div className="absolute bg-white inset-0 rounded-[16px]" />
        <div className="absolute inset-0 mix-blend-luminosity rounded-[16px]" style={{ backgroundImage: "linear-gradient(135.548deg, rgba(0, 0, 0, 0) 1.2433%, rgba(0, 0, 0, 0.08) 95.008%)" }} />
      </div>
      <div className="overflow-clip relative rounded-[inherit] size-full">
        <div className="absolute bg-[rgba(27,32,40,0.2)] h-[32px] left-[28px] rounded-[8px] top-[28px] w-[142px]" data-name="Header section" />
        <div className="absolute bg-[rgba(27,32,40,0.2)] inset-[80px_28px_28px_28px] opacity-50 rounded-[8px]" data-name="Primary section" />
      </div>
      <div aria-hidden="true" className="absolute border-[1.5px] border-[rgba(73,89,110,0.2)] border-solid inset-0 pointer-events-none rounded-[16px] shadow-[15px_128px_36px_0px_rgba(0,0,0,0),10px_82px_33px_0px_rgba(0,0,0,0.01),5px_46px_28px_0px_rgba(0,0,0,0.04),2px_20px_21px_0px_rgba(0,0,0,0.06),1px_5px_11px_0px_rgba(0,0,0,0.08)]" />
    </div>
  );
}

function Popular() {
  return (
    <div className="absolute left-[20px] size-[32px] top-[20px]" data-name="popular">
      <svg className="absolute block inset-0 size-full" fill="none" preserveAspectRatio="none" viewBox="0 0 32 32">
        <g id="popular">
          <path d={svgPaths.p3e56e300} id="Icon" stroke="var(--stroke-0, #0050C1)" strokeLinejoin="round" strokeWidth="1.5" />
        </g>
      </svg>
    </div>
  );
}

function Container4() {
  return (
    <div className="-translate-x-1/2 absolute h-[72px] left-[calc(50%-143.5px)] rounded-[16px] top-[289px] w-[231px]" data-name="Container">
      <div aria-hidden="true" className="absolute inset-0 pointer-events-none rounded-[16px]">
        <div className="absolute inset-0 rounded-[16px]" style={{ backgroundImage: "linear-gradient(90deg, rgba(37, 74, 126, 0.09) 0%, rgba(37, 74, 126, 0.09) 100%), linear-gradient(90deg, rgb(255, 255, 255) 0%, rgb(255, 255, 255) 100%)" }} />
        <div className="absolute inset-0 mix-blend-luminosity rounded-[16px]" style={{ backgroundImage: "linear-gradient(167.372deg, rgba(250, 252, 255, 0) 1.2433%, rgba(0, 84, 173, 0.05) 95.008%)" }} />
      </div>
      <div className="overflow-clip relative rounded-[inherit] size-full">
        <div className="-translate-y-1/2 absolute flex flex-col font-['Public_Sans:Bold',sans-serif] font-bold justify-center leading-[0] left-[69px] overflow-hidden text-[#1b2028] text-[20px] text-ellipsis top-[38px] tracking-[-0.4px] w-[142px] whitespace-nowrap">
          <p className="leading-[24px] overflow-hidden text-ellipsis">Histórico</p>
        </div>
        <Popular />
      </div>
      <div aria-hidden="true" className="absolute border-[1.5px] border-[rgba(73,89,110,0.2)] border-solid inset-0 pointer-events-none rounded-[16px] shadow-[15px_128px_36px_0px_rgba(0,0,0,0),10px_82px_33px_0px_rgba(0,0,0,0.01),5px_46px_28px_0px_rgba(0,0,0,0.04),2px_20px_21px_0px_rgba(0,0,0,0.06),1px_5px_11px_0px_rgba(0,0,0,0.08)]" />
    </div>
  );
}

function Settings() {
  return (
    <div className="absolute left-[20px] size-[32px] top-[20px]" data-name="settings">
      <svg className="absolute block inset-0 size-full" fill="none" preserveAspectRatio="none" viewBox="0 0 32 32">
        <g clipPath="url(#clip0_13_163)" id="settings">
          <g id="Icon">
            <path d={svgPaths.p34392700} stroke="var(--stroke-0, #0050C1)" strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.5" />
            <path d={svgPaths.p3a531880} stroke="var(--stroke-0, #0050C1)" strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.5" />
          </g>
        </g>
        <defs>
          <clipPath id="clip0_13_163">
            <rect fill="white" height="32" width="32" />
          </clipPath>
        </defs>
      </svg>
    </div>
  );
}

function Container5() {
  return (
    <div className="-translate-x-1/2 absolute h-[72px] left-[calc(50%+146.5px)] rounded-[16px] top-[158px] w-[231px]" data-name="Container">
      <div aria-hidden="true" className="absolute inset-0 pointer-events-none rounded-[16px]">
        <div className="absolute inset-0 rounded-[16px]" style={{ backgroundImage: "linear-gradient(90deg, rgba(37, 74, 126, 0.09) 0%, rgba(37, 74, 126, 0.09) 100%), linear-gradient(90deg, rgb(255, 255, 255) 0%, rgb(255, 255, 255) 100%)" }} />
        <div className="absolute inset-0 mix-blend-luminosity rounded-[16px]" style={{ backgroundImage: "linear-gradient(167.372deg, rgba(250, 252, 255, 0) 1.2433%, rgba(0, 84, 173, 0.05) 95.008%)" }} />
      </div>
      <div className="overflow-clip relative rounded-[inherit] size-full">
        <div className="-translate-y-1/2 absolute flex flex-col font-['Public_Sans:Bold',sans-serif] font-bold justify-center leading-[0] left-[69px] overflow-hidden text-[#1b2028] text-[20px] text-ellipsis top-[36px] tracking-[-0.4px] w-[142px] whitespace-nowrap">
          <p className="leading-[24px] overflow-hidden text-ellipsis">Detalhes</p>
        </div>
        <Settings />
      </div>
      <div aria-hidden="true" className="absolute border-[1.5px] border-[rgba(73,89,110,0.2)] border-solid inset-0 pointer-events-none rounded-[16px] shadow-[15px_128px_36px_0px_rgba(0,0,0,0),10px_82px_33px_0px_rgba(0,0,0,0.01),5px_46px_28px_0px_rgba(0,0,0,0.04),2px_20px_21px_0px_rgba(0,0,0,0.06),1px_5px_11px_0px_rgba(0,0,0,0.08)]" />
    </div>
  );
}

function UiSnippet1() {
  return (
    <div className="h-[405px] relative shrink-0 w-full" data-name="ui snippet">
      <Container3 />
      <Container4 />
      <Container5 />
    </div>
  );
}

function ImageDiv1() {
  return (
    <div className="content-stretch flex flex-[1_0_0] flex-col items-start min-h-px relative w-full" data-name="Image div">
      <UiSnippet1 />
    </div>
  );
}

function Bento1() {
  return (
    <div className="bg-[rgba(37,74,126,0.09)] flex-[1_0_0] h-[480px] min-w-px relative rounded-[16px]" data-name="bento2">
      <div className="content-stretch flex flex-col items-end overflow-clip pt-[40px] relative rounded-[inherit] size-full">
        <TextContent1 />
        <ImageDiv1 />
      </div>
      <div aria-hidden="true" className="absolute border-[1.5px] border-[rgba(0,0,0,0)] border-solid inset-0 pointer-events-none rounded-[16px]" />
    </div>
  );
}

function Row() {
  return (
    <div className="content-stretch flex gap-[32px] items-start relative shrink-0 w-full" data-name="row1">
      <Bento />
      <Bento1 />
    </div>
  );
}

function TextContent2() {
  return (
    <div className="relative shrink-0 w-full" data-name="textContent">
      <div className="content-stretch flex flex-col gap-[10px] items-start px-[40px] relative size-full text-[#1b2028]">
        <p className="font-['Public_Sans:Bold',sans-serif] font-bold leading-[32px] relative shrink-0 text-[28px] tracking-[-0.56px] w-full">Lembretes Automáticos</p>
        <p className="font-['Public_Sans:Regular',sans-serif] font-normal leading-[24px] relative shrink-0 text-[20px] tracking-[-0.1px] w-full">Receba lembretes para nunca esquecer uma revisão.</p>
      </div>
    </div>
  );
}

function TitleContainer() {
  return (
    <div className="bg-[#1677ff] content-stretch flex h-[48px] items-center justify-center px-[16px] py-[8px] relative rounded-[8px] shrink-0" data-name="Title Container">
      <div className="flex flex-col font-['Public_Sans:Bold',sans-serif] font-bold justify-center leading-[0] overflow-hidden relative shrink-0 text-[#000205] text-[20px] text-ellipsis tracking-[-0.4px] whitespace-nowrap">
        <p className="leading-[24px] overflow-hidden text-ellipsis">Lembretes</p>
      </div>
    </div>
  );
}

function ButtonContainer() {
  return (
    <div className="absolute content-stretch flex gap-[24px] items-center right-[40px] top-[40px]" data-name="Button Container">
      <div className="bg-[rgba(27,32,40,0.2)] h-[32px] opacity-50 rounded-[8px] shrink-0 w-[139px]" data-name="Left Button" />
      <div className="bg-[rgba(27,32,40,0.2)] h-[32px] opacity-50 rounded-[8px] shrink-0 w-[105px]" data-name="Right Button" />
      <TitleContainer />
    </div>
  );
}

function Content1() {
  return (
    <div className="absolute bottom-[-120.5px] h-[493.5px] left-[-51px] mask-alpha mask-intersect mask-no-clip mask-no-repeat mask-position-[51px_-22px] mask-size-[557px_395px] rounded-[24px] w-[579px]" style={{ maskImage: `url('${imgContent}')` }} data-name="Content">
      <div aria-hidden="true" className="absolute inset-0 pointer-events-none rounded-[24px]">
        <div className="absolute bg-white inset-0 rounded-[24px]" />
        <div className="absolute inset-0 mix-blend-luminosity rounded-[24px]" style={{ backgroundImage: "linear-gradient(148.507deg, rgba(250, 252, 255, 0) 1.2433%, rgba(0, 84, 173, 0.05) 95.008%)" }} />
      </div>
      <div className="overflow-clip relative rounded-[inherit] size-full">
        <ButtonContainer />
        <div className="absolute bg-[rgba(27,32,40,0.2)] inset-[152px_40px_45.5px_0] opacity-50 rounded-[8px]" data-name="Content Background" />
      </div>
      <div aria-hidden="true" className="absolute border-[2.25px] border-[rgba(73,89,110,0.2)] border-solid inset-0 pointer-events-none rounded-[24px] shadow-[19.789px_168.863px_47.493px_0px_rgba(0,0,0,0),13.192px_108.178px_43.535px_0px_rgba(0,0,0,0.01),6.596px_60.685px_36.939px_0px_rgba(0,0,0,0.04),2.638px_26.385px_27.704px_0px_rgba(0,0,0,0.06),1.319px_6.596px_14.512px_0px_rgba(0,0,0,0.08)]" />
    </div>
  );
}

function Window() {
  return (
    <div className="absolute bottom-[-120.5px] contents left-[-51px]" data-name="Window">
      <Content1 />
    </div>
  );
}

function UiSnippet2() {
  return (
    <div className="h-[405px] overflow-clip relative shrink-0 w-full" data-name="ui snippet">
      <Window />
    </div>
  );
}

function ImageDiv2() {
  return (
    <div className="content-stretch flex flex-[1_0_0] flex-col items-start min-h-px relative w-full" data-name="Image div">
      <UiSnippet2 />
    </div>
  );
}

function Bento2() {
  return (
    <div className="bg-[rgba(37,74,126,0.09)] flex-[1_0_0] h-[480px] min-w-px relative rounded-[16px]" data-name="bento3">
      <div className="content-stretch flex flex-col items-end overflow-clip pt-[40px] relative rounded-[inherit] size-full">
        <TextContent2 />
        <ImageDiv2 />
      </div>
      <div aria-hidden="true" className="absolute border-[1.5px] border-[rgba(0,0,0,0)] border-solid inset-0 pointer-events-none rounded-[16px]" />
    </div>
  );
}

function TextContent3() {
  return (
    <div className="relative shrink-0 w-full" data-name="textContent">
      <div className="content-stretch flex flex-col gap-[10px] items-start px-[40px] relative size-full text-[#1b2028]">
        <p className="font-['Public_Sans:Bold',sans-serif] font-bold leading-[32px] relative shrink-0 text-[28px] tracking-[-0.56px] w-full">Gratuito para Sempre</p>
        <p className="font-['Public_Sans:Regular',sans-serif] font-normal leading-[24px] relative shrink-0 text-[20px] tracking-[-0.1px] w-full">Use todas as funcionalidades sem pagar nada.</p>
      </div>
    </div>
  );
}

function ButtonContainer1() {
  return (
    <div className="bg-[#1677ff] content-stretch flex h-[48px] items-center justify-center px-[16px] py-[8px] relative rounded-[8px] shrink-0" data-name="Button Container">
      <div className="flex flex-col font-['Public_Sans:Bold',sans-serif] font-bold justify-center leading-[0] overflow-hidden relative shrink-0 text-[#000205] text-[20px] text-ellipsis tracking-[-0.4px] whitespace-nowrap">
        <p className="leading-[24px] overflow-hidden text-ellipsis">Ilimitado</p>
      </div>
    </div>
  );
}

function Container6() {
  return (
    <div className="-translate-x-1/2 absolute left-[calc(50%-0.5px)] rounded-[16px] top-[33px]" data-name="Container">
      <div aria-hidden="true" className="absolute inset-0 pointer-events-none rounded-[16px]">
        <div className="absolute bg-white inset-0 rounded-[16px]" />
        <div className="absolute inset-0 mix-blend-luminosity rounded-[16px]" style={{ backgroundImage: "linear-gradient(140.875deg, rgba(250, 252, 255, 0) 1.2433%, rgba(0, 84, 173, 0.05) 95.008%)" }} />
      </div>
      <div className="content-stretch flex flex-col gap-[20px] items-start overflow-clip p-[28px] relative rounded-[inherit] size-full">
        <div className="flex flex-col font-['Public_Sans:Bold',sans-serif] font-bold justify-center leading-[0] min-w-full overflow-hidden relative shrink-0 text-[#1b2028] text-[28px] text-ellipsis tracking-[-0.56px] w-[min-content] whitespace-nowrap">
          <p className="leading-[32px] overflow-hidden text-ellipsis">Gratuito</p>
        </div>
        <div className="bg-[rgba(27,32,40,0.2)] h-[104px] opacity-50 rounded-[8px] shrink-0 w-[301px]" data-name="Content Block" />
        <div className="bg-[rgba(27,32,40,0.2)] h-[104px] opacity-50 rounded-[8px] shrink-0 w-[301px]" data-name="Content Block" />
        <ButtonContainer1 />
      </div>
      <div aria-hidden="true" className="absolute border-[1.5px] border-[rgba(73,89,110,0.2)] border-solid inset-0 pointer-events-none rounded-[16px] shadow-[15px_128px_36px_0px_rgba(0,0,0,0),10px_82px_33px_0px_rgba(0,0,0,0.01),5px_46px_28px_0px_rgba(0,0,0,0.04),2px_20px_21px_0px_rgba(0,0,0,0.06),1px_5px_11px_0px_rgba(0,0,0,0.08)]" />
    </div>
  );
}

function UiSnippet3() {
  return (
    <div className="h-[405px] relative shrink-0 w-full" data-name="ui snippet">
      <Container6 />
    </div>
  );
}

function ImageDiv3() {
  return (
    <div className="content-stretch flex flex-[1_0_0] flex-col items-start min-h-px relative w-full" data-name="Image div">
      <UiSnippet3 />
    </div>
  );
}

function Bento3() {
  return (
    <div className="bg-[rgba(37,74,126,0.09)] flex-[1_0_0] h-[480px] min-w-px relative rounded-[16px]" data-name="bento4">
      <div className="content-stretch flex flex-col items-end overflow-clip pt-[40px] relative rounded-[inherit] size-full">
        <TextContent3 />
        <ImageDiv3 />
      </div>
      <div aria-hidden="true" className="absolute border-[1.5px] border-[rgba(0,0,0,0)] border-solid inset-0 pointer-events-none rounded-[16px]" />
    </div>
  );
}

function Row1() {
  return (
    <div className="content-stretch flex gap-[32px] items-start relative shrink-0 w-full" data-name="row2">
      <Bento2 />
      <Bento3 />
    </div>
  );
}

function BentoGrid() {
  return (
    <div className="content-stretch flex flex-col gap-[32px] items-start relative shrink-0 w-full" data-name="bentoGrid">
      <Row />
      <Row1 />
    </div>
  );
}

function SoftwareMarketingShortFeatureBoxes() {
  return (
    <div className="bg-[#fafcff] content-stretch flex flex-col gap-[52px] items-center justify-center py-[52px] relative shrink-0 w-full z-[2]" data-name="Software Marketing Short Feature Boxes">
      <div className="w-full max-w-[1440px] px-[48px] flex flex-col gap-[52px]">
        <p className="font-['Public_Sans:Bold',sans-serif] font-bold leading-[52px] relative shrink-0 text-[#1b2028] text-[48px] text-center tracking-[-0.96px]">Funcionalidades do MotoRev</p>
        <BentoGrid />
      </div>
      <div className="absolute bottom-0 h-0 left-0 right-0" data-name="divider">
        <svg className="absolute block inset-0 size-full" fill="none" preserveAspectRatio="none" viewBox="0 0 32 32">
          <g id="divider" />
        </svg>
      </div>
    </div>
  );
}

function TextContent4() {
  return (
    <div className="content-stretch flex flex-col gap-[20px] items-start relative shrink-0 text-[#1b2028] w-full" data-name="textContent">
      <p className="font-['Public_Sans:Bold',sans-serif] font-bold leading-[52px] relative shrink-0 text-[48px] tracking-[-0.96px] w-full">Para Donos de Moto</p>
      <p className="font-['Public_Sans:Regular',sans-serif] font-normal leading-[32px] relative shrink-0 text-[24px] tracking-[-0.12px] w-full">Gerencie suas revisões com facilidade e mantenha sua moto sempre em dia.</p>
    </div>
  );
}

function ButtonLarge() {
  const navigate = useNavigate();

  return (
    <div className="content-stretch flex items-center justify-center relative shrink-0" data-name="buttonLarge">
      <Button type="primary" size="large" style={{ height: '52px', fontSize: '20px', padding: '0 32px' }} onClick={() => navigate(PATHS.CADASTRO)}>Saiba Mais</Button>
    </div>
  );
}

function Div1() {
  return (
    <div className="content-stretch flex flex-[1_0_0] flex-col gap-[40px] items-start min-w-px relative" data-name="div">
      <TextContent4 />
      <ButtonLarge />
    </div>
  );
}

function AMotorcycleOwnerInspectingTheirMotorcycleInABrightGarageSurroundedByToolsAndEquipment() {
  return (
    <div className="flex-[1_0_0] h-[485px] min-w-px pointer-events-none relative rounded-[16px]" data-name="A motorcycle owner inspecting their motorcycle in a bright garage, surrounded by tools and equipment.">
      <img alt="" className="absolute inset-0 max-w-none object-cover rounded-[16px] size-full" src={imgAMotorcycleOwnerInspectingTheirMotorcycleInABrightGarageSurroundedByToolsAndEquipment} />
      <div aria-hidden="true" className="absolute border-[1.5px] border-[rgba(0,0,0,0)] border-solid inset-0 rounded-[16px]" />
    </div>
  );
}

function InfoBlockSizeLayout() {
  return (
    <div className="bg-[#fafcff] content-stretch flex items-center justify-center h-[720px] py-[52px] relative shrink-0 w-full z-[2]" data-name="infoBlock{-size,layout}">
      <div className="w-full max-w-[1440px] px-[48px] flex gap-[96px] items-center">
        <Div1 />
        <AMotorcycleOwnerInspectingTheirMotorcycleInABrightGarageSurroundedByToolsAndEquipment />
      </div>
      <div className="absolute bottom-0 h-0 left-0 right-0" data-name="divider">
        <svg className="absolute block inset-0 size-full" fill="none" preserveAspectRatio="none" viewBox="0 0 32 32">
          <g id="divider" />
        </svg>
      </div>
    </div>
  );
}

function AMechanicWearingABlueJumpsuitInspectingAMotorcycleInAWellLitWorkshopSurroundedByToolsAndEquipment() {
  return (
    <div className="flex-[1_0_0] h-[485px] min-w-px pointer-events-none relative rounded-[16px]" data-name="A mechanic wearing a blue jumpsuit inspecting a motorcycle in a well-lit workshop surrounded by tools and equipment.">
      <img alt="" className="absolute inset-0 max-w-none object-cover rounded-[16px] size-full" src={imgAMechanicWearingABlueJumpsuitInspectingAMotorcycleInAWellLitWorkshopSurroundedByToolsAndEquipment} />
      <div aria-hidden="true" className="absolute border-[1.5px] border-[rgba(0,0,0,0)] border-solid inset-0 rounded-[16px]" />
    </div>
  );
}

function TextContent5() {
  return (
    <div className="content-stretch flex flex-col gap-[20px] items-start relative shrink-0 text-[#1b2028] w-full" data-name="textContent">
      <p className="font-['Public_Sans:Bold',sans-serif] font-bold leading-[52px] relative shrink-0 text-[48px] tracking-[-0.96px] w-full">Para Oficinas</p>
      <p className="font-['Public_Sans:Regular',sans-serif] font-normal leading-[32px] relative shrink-0 text-[24px] tracking-[-0.12px] w-full">Organize os agendamentos e ofereça um serviço ainda melhor.</p>
    </div>
  );
}

function ButtonLarge1() {
  const navigate = useNavigate();

  return (
    <div className="content-stretch flex items-center justify-center relative shrink-0" data-name="buttonLarge">
      <Button type="primary" size="large" style={{ height: '52px', fontSize: '20px', padding: '0 32px' }} onClick={() => navigate(PATHS.CADASTRO)}>Saiba Mais</Button>
    </div>
  );
}

function Div2() {
  return (
    <div className="content-stretch flex flex-[1_0_0] flex-col gap-[40px] items-start min-w-px relative" data-name="div">
      <TextContent5 />
      <ButtonLarge1 />
    </div>
  );
}

function InfoBlockSizeLayout1() {
  return (
    <div className="bg-[#fafcff] content-stretch flex items-center justify-center h-[720px] py-[52px] relative shrink-0 w-full z-[1]" data-name="infoBlock{-size,layout}">
      <div className="w-full max-w-[1440px] px-[48px] flex gap-[96px] items-center">
        <AMechanicWearingABlueJumpsuitInspectingAMotorcycleInAWellLitWorkshopSurroundedByToolsAndEquipment />
        <Div2 />
      </div>
      <div className="absolute bottom-0 h-0 left-0 right-0" data-name="divider">
        <svg className="absolute block inset-0 size-full" fill="none" preserveAspectRatio="none" viewBox="0 0 32 32">
          <g id="divider" />
        </svg>
      </div>
    </div>
  );
}

function LandingPageTwoFeatureDescriptionsWithImages() {
  return (
    <div className="content-stretch flex flex-col isolate items-start relative shrink-0 w-full z-[1]" data-name="Landing Page Two Feature Descriptions with Images">
      <InfoBlockSizeLayout />
      <InfoBlockSizeLayout1 />
    </div>
  );
}

function MainContent() {
  return (
    <div className="content-stretch flex flex-[1_0_0] flex-col isolate items-start min-w-px relative z-[1]" data-name="Main Content">
      <LandingPageHeroWithTaglineAndDesktopAppMockup />
      <SoftwareMarketingShortFeatureBoxes />
      <LandingPageTwoFeatureDescriptionsWithImages />
    </div>
  );
}

function Container() {
  return (
    <div className="content-stretch flex isolate items-start relative shrink-0 w-full z-[2]" data-name="Container">
      <MainContent />
    </div>
  );
}

function BasicFooter() {
  return <div className="bg-[#fafcff] h-[48px] shrink-0 w-full z-[1]" data-name="Basic Footer" />;
}

export default function MotoRevLandingPage() {
  return (
    <div className="bg-[#fafcff] content-stretch flex flex-col isolate items-start relative size-full" data-name="MotoRev — Landing Page">
      <SoftwareCompanyHeader />
      <Container />
      <BasicFooter />
    </div>
  );
}