using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Text;
using Newtonsoft.Json;



internal class Program
{
    class DictionaryManager
    {
        private Dictionary<string, List<string>> words;
        private string filePath;

        public string FilePath => filePath;

        public bool Counter1()
        {
            if(words.Count() == 0)
            {
                Console.WriteLine("Словник порожній.");
                return true;
            }
            return false;
        }

        public DictionaryManager(string filePath)
        {
            this.filePath = filePath;
            words = new Dictionary<string, List<string>>();

            LoadFromFile();
        }

        private void LoadFromFile()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                words = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json) ?? new Dictionary<string, List<string>>();
            }
            else
                SaveToFile();   
        }

        private void SaveToFile()
        {
            string json = JsonConvert.SerializeObject(words);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        public void AddWord(string word, string translation)
        {
            if (!words.ContainsKey(word))
                words[word] = new List<string>();

            if (!words[word].Contains(translation))
            {
                words[word].Add(translation);
                SaveToFile();
                Console.WriteLine($"Додано: {word} - {translation}");
            }
            else
                Console.WriteLine("Такий переклад вже існує.");
        }

        public bool ShowDictionary()
        {
            if (words.Count == 0)
            {
                Console.WriteLine("Словник порожній.");
                return false;
            }

            Console.WriteLine("\nВсі слова та переклади в словнику: \n");
            foreach (var item in words)
            {
                Console.WriteLine($"{item.Key}: {string.Join(", ", item.Value)}");
            }
            return true;
        }

        public void FindTranslation(string word)
        {
            if (words.ContainsKey(word))
                Console.WriteLine($"{word}: {string.Join(", ", words[word])}");
            else
                Console.WriteLine($"Слово '{word}' не знайдено.");
        }

        public void RemoveWord(string word)
        {
            if (words.Remove(word))
            {
                SaveToFile();
                Console.WriteLine($"Слово '{word}' видалено.");
            }
            else
                Console.WriteLine($"Слово '{word}' не знайдено.");
        }

        public void RemoveTranslation(string word, string translation)
        {
            if (words.ContainsKey(word))
            {
                if (words[word].Count == 1)
                {
                    Console.WriteLine($"Неможливо видалити '{translation}', оскільки це єдиний переклад для '{word}'.");
                    return;
                }

                if (words[word].Contains(translation))
                {
                    words[word].Remove(translation);
                    SaveToFile();
                    Console.WriteLine($"Переклад '{translation}' для слова '{word}' видалено.");
                }
                else
                    Console.WriteLine($"Переклад '{translation}' не знайдено.");
            }
            else
                Console.WriteLine($"Слово '{word}' не знайдено.");
        }

        public void DeleteDictionary()
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine($"Словник '{filePath}' був успішно видалений.");
            }
            else
                Console.WriteLine($"Словник '{filePath}' не знайдено.");
        }

        public void ReplaceWord(string oldWord, string newWord)
        {
            if (!words.ContainsKey(oldWord))
            {
                Console.WriteLine($"Слово '{oldWord}' не знайдено.");
                return;
            }

            if (words.ContainsKey(newWord))
            {
                Console.WriteLine($"Слово '{newWord}' вже існує в словнику.");
                return;
            }

            List<string> translations = words[oldWord];
            words.Remove(oldWord); 
            words[newWord] = translations; 

            SaveToFile();
            Console.WriteLine($"Слово '{oldWord}' замінено на '{newWord}'.");
        }

        public void ReplaceTranslation(string word, string oldTranslation, string newTranslation)
        {
            if (!words.ContainsKey(word))
            {
                Console.WriteLine($"Слово '{word}' не знайдено.");
                return;
            }

            if (!words[word].Contains(oldTranslation))
            {
                Console.WriteLine($"Переклад '{oldTranslation}' для слова '{word}' не знайдено.");
                return;
            }

            if (words[word].Contains(newTranslation))
            {
                Console.WriteLine($"Переклад '{newTranslation}' вже існує для слова '{word}'.");
                return;
            }

            int index = words[word].IndexOf(oldTranslation);
            words[word][index] = newTranslation;

            SaveToFile();
            Console.WriteLine($"Переклад '{oldTranslation}' для слова '{word}' замінено на '{newTranslation}'.");
        }

    }

    class DictionaryFileManager
    {
        private string dictionaryListFilePath = "dictionaries.json";

        public List<string> LoadDictionaryPaths()
        {
            if (File.Exists(dictionaryListFilePath))
            {
                string json = File.ReadAllText(dictionaryListFilePath, Encoding.UTF8);
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            return new List<string>();
        }

        public void SaveDictionaryPaths(List<string> dictionaryPaths)
        {
            string json = JsonConvert.SerializeObject(dictionaryPaths);
            File.WriteAllText(dictionaryListFilePath, json, Encoding.UTF8);
        }
    }

    private static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.Unicode;
        Console.InputEncoding = Encoding.Unicode;



        DictionaryFileManager dictionaryFileManager = new DictionaryFileManager();
        List<DictionaryManager> dictionaries = new List<DictionaryManager>();

        List<string> dictionaryPaths = dictionaryFileManager.LoadDictionaryPaths();
        foreach (var path in dictionaryPaths)
            dictionaries.Add(new DictionaryManager(path));

        while (true)
        {
            Console.Clear();
            Console.WriteLine("\nМеню:");
            Console.WriteLine("1 - Вибрати існуючий словник");
            Console.WriteLine("2 - Додати новий словник");
            Console.WriteLine("3 - Видалити словник");
            Console.WriteLine("0 - Вийти");
            Console.Write("\nОберіть дію: ");
            string choice = Console.ReadLine()!;

            switch (choice)
            {
                case "1":
                    if (dictionaries.Count == 0)
                    {
                        Console.WriteLine("\nНемає доступних словників. Додайте хоча б один.");
                        break;
                    }

                    Console.WriteLine("Оберіть словник:");
                    for (int i = 0; i < dictionaries.Count; i++)
                    {
                        string fileName = Path.GetFileName(dictionaries[i].FilePath);
                        Console.WriteLine($"{i + 1} - {fileName}");
                    }


                    Console.Write("Оберіть номер словника: ");
                    int dictionaryChoice;
                    if (int.TryParse(Console.ReadLine(), out dictionaryChoice) && dictionaryChoice > 0 && dictionaryChoice <= dictionaries.Count)
                    {
                        DictionaryManager selectedDictionary = dictionaries[dictionaryChoice - 1];
                        ManageDictionary(selectedDictionary);
                    }
                    else
                        Console.WriteLine("Невірний вибір.");
                    break;

                case "2":
                    Console.Write("\nВведіть ім'я для нового словника (наприклад, ukr_engl.json): ");
                    string newFileName = Console.ReadLine()!;
                    if (string.IsNullOrWhiteSpace(newFileName))
                    {
                        Console.WriteLine("Некоректне ім'я файлу.");
                        break;
                    }

                    if (File.Exists(newFileName))
                        Console.WriteLine("Цей словник вже існує.");
                    else
                    {
                        DictionaryManager newDictionary = new DictionaryManager(newFileName);
                        dictionaries.Add(newDictionary);
                        dictionaryPaths.Add(newFileName); 
                        dictionaryFileManager.SaveDictionaryPaths(dictionaryPaths); 
                        Console.WriteLine($"Новий словник '{newFileName}' додано.");
                    }
                    break;

                case "3":
                    if (dictionaries.Count == 0)
                    {
                        Console.WriteLine("\nНемає доступних словників для видалення.");
                        break;
                    }

                    Console.WriteLine("Оберіть словник для видалення:");
                    for (int i = 0; i < dictionaries.Count; i++)
                    {
                        string fileName = Path.GetFileName(dictionaries[i].FilePath);
                        Console.WriteLine($"{i + 1} - {fileName}");
                    }

                    Console.Write("Оберіть номер словника для видалення: ");
                    int deleteChoice;
                    if (int.TryParse(Console.ReadLine(), out deleteChoice) && deleteChoice > 0 && deleteChoice <= dictionaries.Count)
                    {
                        DictionaryManager selectedDictionary = dictionaries[deleteChoice - 1];
                        selectedDictionary.DeleteDictionary();
                        dictionaries.RemoveAt(deleteChoice - 1);
                        dictionaryPaths.RemoveAt(deleteChoice - 1); 
                        dictionaryFileManager.SaveDictionaryPaths(dictionaryPaths);
                    }
                    else
                        Console.WriteLine("Невірний вибір.");
                    break;

                case "0":
                    Console.WriteLine("\nДякую що користувались нашим словником\nДо побачення!");
                    return;

                default:
                    Console.WriteLine("Невідомий вибір, спробуйте ще раз.");
                    break;
            }
            Console.Write("\nНатисніть Enter для продовження...");
            Console.ReadLine();
        }
    }

    private static void ManageDictionary(DictionaryManager dictionary)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Меню:");
            Console.WriteLine("1 - Додати слово");
            Console.WriteLine("2 - Показати словник");
            Console.WriteLine("3 - Знайти переклад слова");
            Console.WriteLine("4 - Видалити слово");
            Console.WriteLine("5 - Видалити переклад");
            Console.WriteLine("6 - Замінити слово");
            Console.WriteLine("7 - Замінити переклад");
            Console.WriteLine("8 - Повернутись до вибору словника");
            Console.WriteLine("0 - Вийти");
            Console.Write("\nОберіть дію: ");

            string choice = Console.ReadLine()!;

            switch (choice)
            {
                case "1":
                    Console.Write("Введіть слово: ");
                    string word = Console.ReadLine()!;
                    if (string.IsNullOrWhiteSpace(word))
                    {
                        Console.WriteLine("Некоректне ведення рядок пустий");
                        break;
                    }
                    Console.Write("Введіть переклад: ");
                    string translation = Console.ReadLine()!;
                    if (string.IsNullOrWhiteSpace(translation))
                    {
                        Console.WriteLine("Некоректне ведення рядок пустий");
                        break;
                    }
                    dictionary.AddWord(word, translation);
                    break;

                case "2":
                    if (!dictionary.ShowDictionary())
                        return;
                    break;

                case "3":
                    if (dictionary.Counter1())
                        break;


                    Console.Write("Введіть слово для пошуку: ");
                    string searchWord = Console.ReadLine()!;
                    dictionary.FindTranslation(searchWord);
                    break;

                case "4":
                    if (dictionary.Counter1())
                        break;

                    Console.Write("Введіть слово для видалення: ");
                    string removeWord = Console.ReadLine()!;
                    if (string.IsNullOrWhiteSpace(removeWord))
                    {
                        Console.WriteLine("Некоректне ведення рядок пустий");
                        break;
                    }
                    dictionary.RemoveWord(removeWord);
                    break;

                case "5":
                    if (dictionary.Counter1())
                        break;

                    Console.Write("Введіть слово: ");
                    string removeTranslationWord = Console.ReadLine()!;
                    if (string.IsNullOrWhiteSpace(removeTranslationWord))
                    {
                        Console.WriteLine("Некоректне ведення рядок пустий");
                        break;
                    }
                    Console.Write("Введіть переклад для видалення: ");
                    string removeTranslation = Console.ReadLine()!;
                    if (string.IsNullOrWhiteSpace(removeTranslation))
                    {
                        Console.WriteLine("Некоректне ведення рядок пустий");
                        break;
                    }
                    dictionary.RemoveTranslation(removeTranslationWord, removeTranslation);
                    break;

                case "6":
                    Console.Write("Введіть слово, яке потрібно замінити: ");
                    string oldWord = Console.ReadLine()!;
                    Console.Write("Введіть нове слово: ");
                    string newWord = Console.ReadLine()!;
                    dictionary.ReplaceWord(oldWord, newWord);
                    break;

                case "7":
                    Console.Write("Введіть слово: ");
                    string wordForReplace = Console.ReadLine()!;
                    if (string.IsNullOrWhiteSpace(wordForReplace))
                    {
                        Console.WriteLine("Некоректне ведення рядок пустий");
                        break;
                    }
                    Console.Write("Введіть переклад, який потрібно замінити: ");
                    string oldTranslation = Console.ReadLine()!;
                    if (string.IsNullOrWhiteSpace(oldTranslation))
                    {
                        Console.WriteLine("Некоректне ведення рядок пустий");
                        break;
                    }
                    Console.Write("Введіть новий переклад: ");
                    string newTranslation = Console.ReadLine()!;
                    if (string.IsNullOrWhiteSpace(newTranslation))
                    {
                        Console.WriteLine("Некоректне ведення рядок пустий");
                        break;
                    }
                    dictionary.ReplaceTranslation(wordForReplace, oldTranslation, newTranslation);
                    break;

                case "8":
                    return;

                case "0":
                    Console.WriteLine("\nДякую що користувались нашим словником\nДо побачення!");
                    return;

                default:
                    Console.WriteLine("Невідомий вибір, спробуйте ще раз.");
                    break;
            }

            Console.Write("\nНатисніть Enter для продовження...");
            Console.ReadLine();
        }
    }
}
