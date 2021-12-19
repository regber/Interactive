# Интерактив для Fallout4

## Программа является результатом рефракторинга и почти полной переделки ранее написанной программы, в результате рефракторинга из программы были полностью удалены элементы написанные на WindowsForms которые были заменены на WPF, упрощены плохо читаемые участки кода, а также исправлены баги.

## Для получения понимания работы программы можно ознакомится с видео по указанной ниже ссылке на котором демонстрируется работа старого варианта программы: https://www.youtube.com/watch?v=6oGmWnqFoy4


## Программа подключается к API стриминговых сервисов таких как Twitch, GoodGame, Peka2tv, а также к сервисам пожертвований таким как DonationAlerts и DonatePay для сбора информации о подписках, донатах, премиум подписках. Также программу можно подключить к API RutonyChat избавившись таким образом от необходимости собирать информацию со стриминговых сервисов по средством самой программы. На основании этой информации программа вызывает в игре тот или иной интерактив воздействуя на значения переменных в памяти игры, таким образом взаимодействуя с внутриигровыми скриптами.
## Для реализации некоторых интерактивных команд и для визуализации интерфейса который отображается непосредственно на стрме были использованы навыки трехмерного моделирования.
## Часть интерактивных команд вызываются через генерирование случайной комбинации вариантов команды по средством рулетки, рулетка реализована по средствам WPF и выводится в окно в отдельном потоке.
## Для манипулирования интерактивными командами в программе реализован конструктор, в котором можно отредактировать порядк запуска и проверки значений переменных в памяти игры перед запуском интерактивной команды.
## Для проигрывания алертов в программе предусмотрено окно алертов, воспроизведение текста из донатного сообщения реализовано по средствам API Yandex SpeechKit.(Для сокращения времени рефракторинга был использован старый вариант с небольшими исправлениями).
## Для отслеживания статистики был использован старый вариант окна статистики (для сокращения времени рефракторинга ранее написанной программы).

## Программа целиком написана на языке C#, использовались сторонние библиотеки для работы с Twitch, для обработки JSON строк, для работы с WebSocket и др. При написании программы старался придерживатся паттерна MVVM. Сохранение данных реализовано через сериализацию классов в XML файл. Некоторая информация со стриминговых сервисов собирается через отправку GET, POST запросов на площадки (было сделано с целью уменьшить кол-во манипуляций необходимых от пользователя).

## Целью рефракторинга было получение практичесских навыков использования WPF и паттерна MVVM.
