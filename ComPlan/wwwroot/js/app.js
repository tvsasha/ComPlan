const apiUrl = "http://ComPlan.somee.com/ComPlan/api";
let currentUser = null;
let users = [];
let selectedUser = null;
let menuTarget = null;

const authBlock = document.getElementById("authBlock");
const userControls = document.getElementById("userControls");
const usersList = document.getElementById("usersList");
const overlay = document.getElementById("overlay");
const editCard = document.getElementById("editCard");
const contextMenu = document.getElementById("contextMenu");
const createBtn = document.getElementById("createBtn");

// Переключение форм
document.getElementById("toRegister").addEventListener("click", () => {
    document.getElementById("loginForm").style.display = "none";
    document.getElementById("registerForm").style.display = "block";
});
document.getElementById("toLogin").addEventListener("click", () => {
    document.getElementById("registerForm").style.display = "none";
    document.getElementById("loginForm").style.display = "block";
});

// Загрузка ролей
async function loadRoles(selectId) {
    try {
        const res = await fetch(`${apiUrl}/roles`);
        const roles = await res.json();
        const select = document.getElementById(selectId);
        select.innerHTML = "";
        roles.forEach(r => {
            const option = document.createElement("option");
            option.value = r.roleId;
            option.textContent = r.roleName;
            select.appendChild(option);
        });
    } catch (err) {
        console.error("Ошибка загрузки ролей", err);
    }
}

// Проверка сессии при загрузке страницы
async function checkSession() {
    const token = localStorage.getItem("sessionToken");
    if (!token) return;

    try {
        const res = await fetch(`${apiUrl}/auth/check-session?token=${token}`);
        if (!res.ok) throw new Error("Сессия недействительна");
        const data = await res.json();
        currentUser = data;
        authBlock.style.display = "none";
        userControls.style.display = "flex";
        usersList.style.display = "flex";
        createBtn.style.display = data.role.roleId === 1 ? "block" : "none";
        await loadRoles("editUserRole");
        loadUsers();
    } catch (err) {
        console.warn("Проверка сессии:", err.message);
        localStorage.removeItem("sessionToken");
    }
}

// Вход
document.getElementById("loginBtn").addEventListener("click", async () => {
    const email = document.getElementById("loginEmail").value;
    const password = document.getElementById("loginPass").value;
    try {
        const res = await fetch(`${apiUrl}/auth/login`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email, password })
        });
        const data = await res.json();
        if (res.ok) {
            currentUser = data;
            localStorage.setItem("sessionToken", data.sessionToken);
            authBlock.style.display = "none";
            userControls.style.display = "flex";
            usersList.style.display = "flex";
            createBtn.style.display = data.role.roleId === 1 ? "block" : "none";
            await loadRoles("editUserRole");
            loadUsers();
        } else {
            document.getElementById("loginError").textContent = data.message || "Ошибка входа";
        }
    } catch (err) {
        console.error("Ошибка входа:", err);
        document.getElementById("loginError").textContent = "Ошибка соединения с сервером";
    }
});

// Регистрация
document.getElementById("regBtn").addEventListener("click", async () => {
    const userName = document.getElementById("regUser").value;
    const email = document.getElementById("regEmail").value;
    const password = document.getElementById("regPass").value;
    const roleId = parseInt(document.getElementById("regRole").value);
    if (!userName || !email || !password) {
        document.getElementById("regError").textContent = "Заполните все поля";
        return;
    }
    const res = await fetch(`${apiUrl}/auth/register`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userName, email, password, roleId })
    });
    const data = await res.json();
    if (res.ok) {
        document.getElementById("regError").textContent = "";
        document.getElementById("toLogin").click();
    } else {
        document.getElementById("regError").textContent = data.message || "Ошибка регистрации";
    }
});

// Выход
document.getElementById("logoutBtn").addEventListener("click", async () => {
    const token = localStorage.getItem("sessionToken");
    if (token && currentUser) {
        await fetch(`${apiUrl}/auth/logout/${currentUser.userId}`, { method: "POST" });
    }
    currentUser = null;
    localStorage.removeItem("sessionToken");
    authBlock.style.display = "block";
    userControls.style.display = "none";
    usersList.style.display = "none";
    usersList.innerHTML = "";
    document.getElementById("loginForm").style.display = "block";
    document.getElementById("registerForm").style.display = "none";
});

// Загрузка пользователей
async function loadUsers() {
    try {
        const token = localStorage.getItem("sessionToken");
        const res = await fetch(`${apiUrl}/users`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        if (!res.ok) throw new Error("Не удалось загрузить пользователей");
        users = await res.json();
        usersList.innerHTML = "";
        if (users.length === 0) {
            usersList.innerHTML = "<li>Нет пользователей</li>";
        } else {
            users.forEach(u => {
                const li = document.createElement("li");
                li.className = "user-card";
                const roleName = u.role ? u.role.roleName : `Роль #${u.roleId}`;
                li.innerHTML = `<div class="user-name">${u.userName}</div>
                                <div class="role-name">${roleName}</div>`;
                li.dataset.userId = u.userId;
                usersList.appendChild(li);

                if (currentUser.role.roleId === 1) {
                    li.addEventListener("contextmenu", e => {
                        e.preventDefault();
                        menuTarget = u;
                        contextMenu.style.top = e.pageY + "px";
                        contextMenu.style.left = e.pageX + "px";
                        contextMenu.style.display = "block";
                    });
                }
            });
        }
        usersList.style.display = "flex";
    } catch (err) {
        console.error("Ошибка загрузки пользователей:", err);
        usersList.innerHTML = "<li>Ошибка загрузки пользователей</li>";
        usersList.style.display = "flex";
    }
}

// Контекстное меню
document.addEventListener("click", () => {
    contextMenu.style.display = "none";
});

document.getElementById("editUserBtn").addEventListener("click", () => {
    contextMenu.style.display = "none";
    openEditCard(menuTarget);
});

document.getElementById("deleteUserBtn").addEventListener("click", async () => {
    contextMenu.style.display = "none";
    try {
        const token = localStorage.getItem("sessionToken");
        const res = await fetch(`${apiUrl}/users/${menuTarget.userId}`, {
            method: "DELETE",
            headers: { "Authorization": `Bearer ${token}` }
        });
        if (!res.ok) throw new Error("Ошибка удаления");
        if (menuTarget.userId === currentUser.userId) {
            currentUser = null;
            localStorage.removeItem("sessionToken");
            authBlock.style.display = "block";
            userControls.style.display = "none";
            usersList.style.display = "none";
            usersList.innerHTML = "";
            return;
        }
        loadUsers();
    } catch (err) {
        alert(err.message || "Ошибка удаления пользователя");
    }
});

// Открытие/закрытие формы редактирования
function openEditCard(user = null) {
    selectedUser = user;
    document.getElementById("editTitle").textContent = user ? "Редактировать пользователя" : "Создать пользователя";
    document.getElementById("editUserName").value = user ? user.userName : "";
    document.getElementById("editUserEmail").value = user ? user.email : "";
    document.getElementById("editUserPass").value = "";
    document.getElementById("editError").textContent = "";
    editCard.style.display = "block";
    overlay.style.display = "block";
    document.getElementById("editUserRole").value = user ? (user.role?.roleId || user.roleId) : "";
}
document.getElementById("cancelEditBtn").addEventListener("click", closeEditCard);
overlay.addEventListener("click", closeEditCard);
function closeEditCard() {
    editCard.style.display = "none";
    overlay.style.display = "none";
}

// Сохранение пользователя
document.getElementById("saveUserBtn").addEventListener("click", async () => {
    const userName = document.getElementById("editUserName").value;
    const email = document.getElementById("editUserEmail").value;
    const password = document.getElementById("editUserPass").value;
    const roleId = parseInt(document.getElementById("editUserRole").value);
    const token = localStorage.getItem("sessionToken");
    try {
        if (selectedUser) {
            const body = { userName, email, roleId };
            if (password) body.password = password;
            const res = await fetch(`${apiUrl}/users/${selectedUser.userId}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                },
                body: JSON.stringify(body)
            });
            if (!res.ok) throw new Error("Ошибка редактирования");
        } else {
            if (!password) throw new Error("Введите пароль");
            const res = await fetch(`${apiUrl}/auth/register`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ userName, email, password, roleId })
            });
            if (!res.ok) throw new Error("Ошибка создания");
        }
        closeEditCard();
        loadUsers();
    } catch (err) {
        document.getElementById("editError").textContent = err.message;
    }
});

// Инициализация
loadRoles("regRole");
loadRoles("editUserRole");
checkSession();