library(tidyverse)

data = read.csv('Grading_Results-ChatGP4.csv')
data %>% glimpse()

data1 = data %>% select(Unit.Test.Grade, ChatGPT.Grade)

t.test(data1$Unit.Test.Grade, data1$ChatGPT.Grade, paired = 'TRUE', alternative = 'two.sided')

ggplot(data1, aes(x = Unit.Test.Grade, y = ChatGPT.Grade)) +
  stat_sum(aes(size = ..n.., alpha = ..n..)) +  # Tamaño de los puntos según la cantidad en cada punto
  scale_size_continuous(name = "Número de puntos", range = c(5, 15)) +  # Añadir leyenda para el tamaño de los puntos
  labs(x = "Nota del Examen Unitario", y = "Nota de ChatGPT", 
       title = "Diagrama de dispersión: Nota del Examen Unitario vs Nota de ChatGPT con recuento de puntos") +
  theme_minimal()

data1$Grade.Difference <- data1$Unit.Test.Grade - data1$ChatGPT.Grade  # Calcular la diferencia

# Graficar el histograma de la diferencia
ggplot(data1, aes(x = Grade.Difference)) +
  geom_histogram(binwidth = 10, fill = "steelblue", color = "black", alpha = 0.7) +
  labs(x = "Diferencia (Nota del Examen Unitario - Nota de ChatGPT)", 
       y = "Frecuencia", 
       title = "Histograma de diferencias de notas entre el Examen Unitario y ChatGPT") +
  theme_minimal()

