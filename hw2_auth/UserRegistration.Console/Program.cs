using UserRegistration.Console.Services;

var userService = new UserService();

while (true)
{
    Console.Clear();

    Console.WriteLine("=================================");
    Console.WriteLine("       USER MANAGEMENT");
    Console.WriteLine("=================================");
    Console.WriteLine("1. Регистрация");
    Console.WriteLine("2. Авторизация");
    Console.WriteLine("3. Редактирование пользователя");
    Console.WriteLine("4. Удаление пользователя");
    Console.WriteLine("5. Список пользователей");
    Console.WriteLine("0. Выход");
    Console.WriteLine("=================================");

    Console.Write("Выберите действие: ");

    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            Register(userService);
            break;

        case "2":
            Login(userService);
            break;

        case "3":
            UpdateUser(userService);
            break;

        case "4":
            DeleteUser(userService);
            break;

        case "5":
            ShowUsers(userService);
            break;

        case "0":
            return;

        default:
            Console.WriteLine("Неизвестная команда.");
            Pause();
            break;
    }
}

static void Register(UserService userService)
{
    Console.Clear();

    Console.WriteLine("=== Регистрация ===");

    Console.Write("Логин: ");
    var login = Console.ReadLine() ?? string.Empty;

    Console.Write("Пароль: ");
    var password = Console.ReadLine() ?? string.Empty;

    Console.Write("Имя: ");
    var firstName = Console.ReadLine() ?? string.Empty;

    Console.Write("Фамилия: ");
    var lastName = Console.ReadLine() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(login) ||
        string.IsNullOrWhiteSpace(password))
    {
        Console.WriteLine("Логин и пароль обязательны.");
        Pause();
        return;
    }

    var user = userService.Register(
        login,
        password,
        firstName,
        lastName);

    if (user == null)
    {
        Console.WriteLine(
            "Пользователь с таким логином уже существует.");
    }
    else
    {
        Console.WriteLine(
            $"Пользователь успешно создан. Id = {user.Id}");
    }

    Pause();
}

static void Login(UserService userService)
{
    Console.Clear();

    Console.WriteLine("=== Авторизация ===");

    Console.Write("Логин: ");
    var login = Console.ReadLine() ?? string.Empty;

    Console.Write("Пароль: ");
    var password = Console.ReadLine() ?? string.Empty;

    var user = userService.Login(login, password);

    if (user == null)
    {
        Console.WriteLine("Неверный логин или пароль.");
    }
    else
    {
        Console.WriteLine(
            $"Добро пожаловать, {user.FirstName} {user.LastName}!");
    }

    Pause();
}

static void UpdateUser(UserService userService)
{
    Console.Clear();

    Console.WriteLine("=== Редактирование пользователя ===");

    Console.Write("Id пользователя: ");

    if (!int.TryParse(Console.ReadLine(), out var id))
    {
        Console.WriteLine("Некорректный Id.");
        Pause();
        return;
    }

    var user = userService.GetById(id);

    if (user == null)
    {
        Console.WriteLine("Пользователь не найден.");
        Pause();
        return;
    }

    Console.Write($"Новое имя ({user.FirstName}): ");
    var firstName = Console.ReadLine();

    Console.Write($"Новая фамилия ({user.LastName}): ");
    var lastName = Console.ReadLine();

    firstName = string.IsNullOrWhiteSpace(firstName)
        ? user.FirstName
        : firstName;

    lastName = string.IsNullOrWhiteSpace(lastName)
        ? user.LastName
        : lastName;

    var result = userService.Update(
        id,
        firstName,
        lastName);

    Console.WriteLine(
        result
            ? "Пользователь успешно изменён."
            : "Не удалось изменить пользователя.");

    Pause();
}

static void DeleteUser(UserService userService)
{
    Console.Clear();

    Console.WriteLine("=== Удаление пользователя ===");

    Console.Write("Id пользователя: ");

    if (!int.TryParse(Console.ReadLine(), out var id))
    {
        Console.WriteLine("Некорректный Id.");
        Pause();
        return;
    }

    var user = userService.GetById(id);

    if (user == null)
    {
        Console.WriteLine("Пользователь не найден.");
        Pause();
        return;
    }

    Console.Write(
        $"Вы действительно хотите удалить {user.Login}? (y/n): ");

    var answer = Console.ReadLine();

    if (answer?.ToLower() != "y")
    {
        Console.WriteLine("Удаление отменено.");
        Pause();
        return;
    }

    var result = userService.Delete(id);

    Console.WriteLine(
        result
            ? "Пользователь удалён."
            : "Не удалось удалить пользователя.");

    Pause();
}

static void ShowUsers(UserService userService)
{
    Console.Clear();

    Console.WriteLine("=== Пользователи ===");

    var users = userService.GetAll();

    if (users.Count == 0)
    {
        Console.WriteLine("Пользователей пока нет.");
        Pause();
        return;
    }

    foreach (var user in users)
    {
        Console.WriteLine(
            $"Id: {user.Id} | " +
            $"Login: {user.Login} | " +
            $"Name: {user.FirstName} {user.LastName}");
    }

    Pause();
}

static void Pause()
{
    Console.WriteLine();
    Console.WriteLine("Нажмите Enter для продолжения...");
    Console.ReadLine();
}