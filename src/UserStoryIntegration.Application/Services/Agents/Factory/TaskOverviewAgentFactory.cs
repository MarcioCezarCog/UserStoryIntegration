namespace UserStoryIntegration.Application.Services.Agents.Factory
{
    /// <summary>
    /// Fábrica para criar o agente de visão geral de tarefas
    /// </summary>
    public class TaskOverviewAgentFactory : AgentFactoryBase
    {
        /// <inheritdoc/>
        protected override string GetInstructionsFileName() => "TaskOverviewInstructions.txt";

        /// <inheritdoc/>
        protected override string GetDefaultInstructions()
        {
            return """
                Sua missão é listar as atividades necessárias, com base em uma estória de usuário.
                Responda em formato de tabela com as colunas: Discipline, Task, Effort Hrs
                Você deve responder sempre em fromato de tabela (Discipline, Task, Effort Hrs)
                """;
        }
    }
}
