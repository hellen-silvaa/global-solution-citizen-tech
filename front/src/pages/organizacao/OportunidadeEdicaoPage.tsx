import { useNavigate, useParams } from 'react-router';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { oportunidadesApi } from '../../api/services';
import { OportunidadeForm, paraCriacao, valoresIniciais } from '../../components/OportunidadeForm';
import { PageHeader } from '../../components/ui/PageHeader';
import { ErrorState, Skeleton } from '../../components/ui/Feedback';
import { useToast } from '../../components/ui/Toast';

/** Criação (/organizacao/oportunidades/nova) e edição (/organizacao/oportunidades/:id/editar). */
export default function OportunidadeEdicaoPage() {
  const { id: idParam } = useParams();
  const id = idParam ? Number(idParam) : undefined;
  const navegar = useNavigate();
  const toast = useToast();
  const queryClient = useQueryClient();
  const q = useQuery({ queryKey: ['oportunidade', id], queryFn: () => oportunidadesApi.obter(id!), enabled: id !== undefined });

  const edicao = id !== undefined;

  return (
    <>
      <PageHeader
        selo={edicao ? 'Editar' : 'Publicar'}
        titulo={edicao ? 'Editar oportunidade' : 'Nova oportunidade'}
        descricao={edicao ? 'Tipo e gratuidade não mudam depois de publicados. As vagas não podem ficar abaixo do número de inscritos.' : 'Quanto mais clara a descrição, mais fácil para a pessoa certa encontrar.'}
      />
      <div className="mx-auto max-w-3xl px-4 py-10 sm:px-6">
        <div className="card p-6 sm:p-8">
          {edicao && q.isLoading ? (
            <Skeleton className="h-96" />
          ) : edicao && (q.isError || !q.data) ? (
            <ErrorState erro={q.error} />
          ) : (
            <OportunidadeForm
              key={q.data?.id ?? 'nova'}
              inicial={valoresIniciais(q.data)}
              edicao={edicao}
              textoEnviar={edicao ? 'Salvar alterações' : 'Publicar oportunidade'}
              aoCancelar={() => navegar(-1)}
              aoEnviar={async (v) => {
                const dados = paraCriacao(v);
                const o = edicao
                  ? await oportunidadesApi.atualizar(id, {
                      titulo: dados.titulo,
                      descricao: dados.descricao,
                      modalidade: dados.modalidade,
                      area: dados.area,
                      bairro: dados.bairro,
                      cargaHoraria: dados.cargaHoraria,
                      vagas: dados.vagas,
                      dataInicio: dados.dataInicio,
                      dataFim: dados.dataFim,
                    })
                  : await oportunidadesApi.criar(dados);
                queryClient.setQueryData(['oportunidade', o.id], o);
                void queryClient.invalidateQueries({ queryKey: ['oportunidades'] });
                void queryClient.invalidateQueries({ queryKey: ['impacto'] });
                toast.sucesso(edicao ? 'Alterações salvas' : 'Oportunidade publicada!', edicao ? undefined : 'Ela já aparece na busca e nas recomendações.');
                navegar(`/oportunidades/${o.id}`);
              }}
            />
          )}
        </div>
      </div>
    </>
  );
}
