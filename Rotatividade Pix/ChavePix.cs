using System;

namespace Rotatividade_Pix 
{
    // Cada vez que você adicionar uma chave na tela, um objeto desta classe será criado.
    public class ChavePix
    {
        // Guarda o nome do dono da chave
        public string Nome { get; set; }

        // Guarda o nome do banco (Ex: Nubank, Inter)
        public string Banco { get; set; }

        // Guarda a chave Pix em si (Pode ser CPF, email, telefone ou aleatória)
        public string Chave { get; set; }

        // Guarda a data e a hora exata em que a chave foi usada pela última vez.
        // O sinal de interrogação (?) significa que este valor pode ser nulo (ou seja, nunca foi usada antes).
        public DateTime? UltimoUso { get; set; }

        // Calcula  se a chave pode ser usada hoje.
        public bool EstaDisponivel
        {
            get
            {
                // Se a data de UltimoUso for nula, ela está 100% disponível retornando true
                if (!UltimoUso.HasValue) return true;

                // Se já foi usada, calcula a diferença entre o momento AGORA (DateTime.Now) e a hora em que foi usada. Se o total de horas for maior ou igual a 48, libera o uso.
                return (DateTime.Now - UltimoUso.Value).TotalHours >= 48;
            }
        }

        // Esta propriedade cria o texto que vai aparecer na coluna "Status" da tabela
        public string Status
        {
            get
            {
                // Se a propriedade de cima disser que está disponível, mostra esse texto:
                if (EstaDisponivel) return "Disponível";

                // Se não estiver disponível, pega a data de uso, soma 48 horas e formata para mostrar o dia, mês e a hora exata da liberação.
                return $"Bloqueada até {UltimoUso.Value.AddHours(48):dd/MM às HH:mm}";
            }
        }
    }
}