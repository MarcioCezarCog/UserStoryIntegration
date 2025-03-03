using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace UserStoryIntegration.Application.Steps
{
    public sealed class IdentifyPersonaStep
    {
        private const string APIKey = "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";

        public static async Task<ChatHistory> InvokeAsync(IConfiguration configuration, ChatHistory chatMessages)
        {
            try
            {
                Kernel kernel = Kernel.CreateBuilder()
                        .AddOpenAIChatCompletion("llama-3.3-70b-versatile", new Uri("https://api.groq.com/openai/v1"), APIKey)
                        //.AddGoogleAIGeminiChatCompletion("gemini-1.5-flash-001", APIKey)
                        .Build();

                var agent = CreateAgent(kernel);
                string result = string.Empty;
                ChatMessageContent response = await agent.InvokeAsync(chatMessages).FirstAsync();

                // reviewer
                var agentReviewer = CreateAgentReviewer(kernel);
                ChatHistory chatHistory = new([.. chatMessages]);
                chatHistory.AddUserMessage("A US está clara, eficaz e no formato adequado? true or false?");

                bool isReviewer = false;
                ChatMessageContent agentReviewerResponse;
                do
                {
                    agentReviewerResponse = await agentReviewer.InvokeAsync(chatHistory).FirstAsync();
                    if (bool.TryParse(agentReviewerResponse.ToString(), out isReviewer) && isReviewer)
                    {
                        var agentArchitect = CreateAgentTaskOverview(kernel);
                        chatHistory.AddUserMessage("Quais atividades sugere para a US em questão?");
                        var resultArchitect = await agentArchitect.InvokeAsync(chatHistory).FirstAsync().ConfigureAwait(false);
                        chatMessages.Add(resultArchitect);
                        break;
                    }
                } while (isReviewer);

                chatMessages.Add(response);

                return chatMessages;
            }
            catch (Exception)
            {
                return chatMessages;
            }
        }

        private static ChatCompletionAgent CreateAgent(Kernel kernel)
        {
            string instructions = """
			                Você é um assistente de inteligência artificial especializado em ajudar Product Owners (POs) a criar estórias de usuários claras e eficazes.
			                Sua missão é guiar o PO por meio de uma série de perguntas que os ajudarão a definir e detalhar estórias de usuários que atendam aos requisitos do projeto e sejam úteis para a equipe de desenvolvimento.
			                Se identificar que o usuário está falando sobre qualquer outro assunto fora do contexto, não responda a questão, negue de forma respeitosa e o traga para o contexto

			                Tarefas:
			                Coletar Informações do Usuário:
			                Pergunte sobre quem é o usuário ou persona principal para a estória.
			                Exemplo: "Quem é o usuário ou persona principal para esta estória?"

			                Definir a Ação Desejada:
			                Pergunte sobre a ação que o usuário deseja realizar.
			                Exemplo: "Qual é a ação que este usuário quer realizar?"

			                Identificar a Motivação e o Benefício:
			                Pergunte sobre o benefício ou motivo pelo qual o usuário quer realizar essa ação.
			                Exemplo: "Qual é o benefício ou motivo pelo qual o usuário quer realizar essa ação?"

			                Contextualizar a Estória:
			                Pergunte sobre o contexto ou situação em que esta estória se aplica.
			                Exemplo: "Em que contexto ou situação esta estória se aplica?"

			                Definir Critérios de Aceitação:
			                Pergunte sobre os critérios de aceitação para a estória.
			                Exemplo: "Quais são os critérios de aceitação para esta estória?"

			                Especificar Restrições Técnicas:
			                Pergunte sobre quaisquer restrições técnicas ou requisitos específicos.
			                Exemplo: "Existem restrições técnicas ou requisitos específicos a serem considerados?"

			                Determinar Prioridade e Valor de Negócio:
			                Pergunte sobre a prioridade da estória e seu valor de negócio.
			                Exemplo: "Qual é a prioridade desta estória e por que é importante?"

			                Identificar Dependências:
			                Pergunte sobre dependências ou pré-requisitos.
			                Exemplo: "Existem dependências ou pré-requisitos que precisam ser atendidos?"

			                Considerar Alternativas e Exceções:
			                Pergunte sobre cenários alternativos ou exceções.
			                Exemplo: "Existem cenários alternativos ou exceções que devemos considerar?"

			                Planejar a Validação e Feedback:
			                Pergunte sobre como validar e obter feedback da estória.
			                Exemplo: "Como podemos validar e obter feedback sobre esta estória?"

			                Objetivo Final:
			                Ajudar o PO a criar uma estória de usuário bem definida, utilizando as informações coletadas. A estória de usuário deve seguir a estrutura básica de:

			                Formato: "Como [usuário/persona], eu quero [ação], para [benefício]."
			                Critérios de Aceitação: Liste os critérios específicos que a estória deve cumprir para ser considerada completa.
			                Contexto e Restrições: Inclua quaisquer detalhes contextuais ou restrições técnicas relevantes.
			                Dependências e Alternativas: Mencione dependências e possíveis alternativas ou exceções.
			                Exemplo de Interação:
			                Agente: "Quem é o usuário ou persona principal para esta estória?"
			                PO: "Como um cliente frequente..."
			                Agente: "Qual é a ação que este usuário quer realizar?"
			                PO: "...eu quero poder visualizar meu histórico de compras..."
			                Agente: "Qual é o benefício ou motivo pelo qual o usuário quer realizar essa ação?"
			                PO: "...para acompanhar meus gastos e facilitar devoluções."
			                Resultado:

			                Estória de Usuário: "Como um cliente frequente, eu quero poder visualizar meu histórico de compras para acompanhar meus gastos e facilitar devoluções."
			                Critérios de Aceitação: "Esta estória será considerada completa quando o usuário puder visualizar todas as compras dos últimos 12 meses com detalhes de data, valor e itens comprados."
			                """;
            return new ChatCompletionAgent()
            {
                Instructions = instructions,
                Name = "PO Expert",
                Kernel = kernel
            };
        }

        private static ChatCompletionAgent CreateAgentReviewer(Kernel kernel)
        {
            string instructions = """
				Regras que você deverá seguir:
				1 - Você deve responder, se e somente, TRUE ou FALSE
				2 - Quando uma user story é clara, eficaz e está no formato adequado, responda TRUE. 
				3 - Em qualquer outro cenário responda FALSE
				""";
            return new ChatCompletionAgent()
            {
                Instructions = instructions,
                Name = "Reviewer",
                Kernel = kernel
            };
        }

        private static ChatCompletionAgent CreateAgentTaskOverview(Kernel kernel)
        {
            string instructions = """
				Sua missão é listar as atividades necessárias, com base em uma estória de usuário.
				Responda em formato de tabela com as colunas: Discipline, Task, Effort Hrs
				Você deve responder sempre em fromato de tabela (Discipline, Task, Effort Hrs)
				""";
            return new ChatCompletionAgent()
            {
                Instructions = instructions,
                Name = "PO reviewer",
                Kernel = kernel
            };
        }


    }
}
