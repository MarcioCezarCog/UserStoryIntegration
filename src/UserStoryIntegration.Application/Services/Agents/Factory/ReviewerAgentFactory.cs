namespace UserStoryIntegration.Application.Services.Agents.Factory
{
    /// <summary>
    /// Fábrica para criar o agente revisor
    /// </summary>
    public class ReviewerAgentFactory : AgentFactoryBase
    {
        /// <inheritdoc/>
        protected override string GetInstructionsFileName() => "ReviewerInstructions.txt";

        /// <inheritdoc/>
        protected override string GetDefaultInstructions()
        {
            return """
                Regras que você deverá seguir:
                1 - Você deve responder, se e somente, TRUE ou FALSE
                2 - Quando uma user story é clara, eficaz e está no formato adequado, responda TRUE. 
                3 - Em qualquer outro cenário responda FALSE
                """;
        }
    }
}
