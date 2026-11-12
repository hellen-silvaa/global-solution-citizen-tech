namespace Oportuniza.Domain.Enums;

public enum Perfil { Admin, Cidadao, Organizacao }

public enum Escolaridade
{
    FundamentalIncompleto, FundamentalCompleto, MedioIncompleto, MedioCompleto,
    Tecnico, SuperiorIncompleto, SuperiorCompleto
}

public enum TipoOrganizacao { Ong, EscolaTecnica, Empresa, PoderPublico, ColetivoComunitario }

public enum AreaConhecimento
{
    Tecnologia, Administracao, Saude, Gastronomia, ConstrucaoCivil,
    Idiomas, Beleza, Logistica, Artes, Empreendedorismo
}

public enum TipoOportunidade { Curso, Oficina, Mentoria, JovemAprendiz }

public enum Modalidade { Presencial, Online, Hibrida }

public enum StatusOportunidade { Aberta, Encerrada, Cancelada }

public enum StatusInscricao { Pendente, Confirmada, Concluida, Cancelada }

public enum StatusProposta { Aberta, Atendida, Arquivada }
