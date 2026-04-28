    # נספח ב': קוד המקור

    ## קוד מתועד של המחלקות העיקריות

    מסמך זה מכיל את הקוד המלא של המחלקות המרכזיות בפרויקט, עם הערות והסברים מפורטים.

    ---

    ## תוכן עניינים

    1. [Models - מודלים](#models)
    2. [DBL - שכבת גישה לנתונים](#dbl)
    3. [API Controllers](#api-controllers)
    4. [Blazor Pages](#blazor-pages)
    5. [Services](#services)

    ---

    ## Models

    ### User.cs - מחלקת משתמש

    ```csharp
    using System;

    namespace Models
    {
        /// <summary>
        /// מחלקה המייצגת משתמש במערכת
        /// כל משתמש יכול להיות משתמש רגיל או מנהל
        /// </summary>
        public class User
        {
            /// <summary>
            /// מזהה ייחודי של המשתמש (Primary Key)
            /// נוצר אוטומטית על ידי המסד נתונים
            /// </summary>
            public int userid { get; set; }
            
            /// <summary>
            /// שם משתמש ייחודי להתחברות
            /// חייב להיות ייחודי במערכת (UNIQUE constraint)
            /// </summary>
            public string username { get; set; }
            
            /// <summary>
            /// סיסמה מוצפנת ב-SHA256
            /// לעולם לא נשמרת בטקסט פשוט
            /// </summary>
            public string password { get; set; }
            
            /// <summary>
            /// כתובת דוא"ל של המשתמש
            /// חייבת להיות ייחודית במערכת
            /// </summary>
            public string email { get; set; }
            
            /// <summary>
            /// תמונת פרופיל של המשתמש
            /// נשמרת כ-BLOB במסד הנתונים
            /// יכולה להיות null אם אין תמונה
            /// </summary>
            public byte[] profilepicture { get; set; }
            
            /// <summary>
            /// האם המשתמש הוא מנהל מערכת
            /// 0 = משתמש רגיל
            /// 1 = מנהל מערכת
            /// </summary>
            public int IsAdmin { get; set; }
            
            /// <summary>
            /// קוד לאיפוס סיסמה
            /// משמש במקרה ששכחו סיסמה
            /// יכול להיות null
            /// </summary>
            public string ResetCode { get; set; }

            /// <summary>
            /// Constructor ריק
            /// יוצר אובייקט User חדש עם ערכי ברירת מחדל
            /// </summary>
            public User()
            {
            }
        }
    }
    ```

    ---

    ### Song.cs - מחלקת שיר

    ```csharp
    using System;

    namespace Models
    {
        /// <summary>
        /// מחלקה המייצגת שיר במערכת
        /// כל שיר כולל את קובץ האודיו עצמו ומטא-דאטה
        /// </summary>
        public class Song
        {
            /// <summary>
            /// מזהה ייחודי של השיר (Primary Key)
            /// </summary>
            public int songID { get; set; }
            
            /// <summary>
            /// כותרת השיר
            /// </summary>
            public string title { get; set; }
            
            /// <summary>
            /// משך השיר בשניות
            /// דוגמה: 180 = 3 דקות
            /// </summary>
            public int duration { get; set; }
            
            /// <summary>
            /// קובץ האודיו המלא כ-BLOB
            /// יכול להיות MP3, WAV, או AAC
            /// </summary>
            public byte[] audioData { get; set; }
            
            /// <summary>
            /// מזהה המשתמש שהעלה את השיר (Foreign Key)
            /// מקושר ל-users.userid
            /// </summary>
            public int userid { get; set; }
            
            /// <summary>
            /// תאריך ושעת העלאה
            /// נקבע אוטומטית על ידי המסד נתונים
            /// </summary>
            public DateTime uploaded { get; set; }
            
            /// <summary>
            /// מספר ההשמעות של השיר
            /// מתעדכן כל פעם שמישהו מאזין
            /// </summary>
            public int plays { get; set; }
            
            /// <summary>
            /// מחרוזת Data URL להשמעה בדפדפן
            /// לא נשמר במסד נתונים - רק לשימוש בממשק
            /// </summary>
            public string audioSource { get; set; }
            
            /// <summary>
            /// ממיר את קובץ האודיו ל-Data URL שהדפדפן יכול להשמיע
            /// </summary>
            /// <param name="data">מערך הבתים של קובץ האודיו</param>
            /// <returns>מחרוזת Data URL בפורמט: data:audio/mpeg;base64,...</returns>
            public string GetAudioSource(byte[] data)
            {
                // בדיקה שיש נתונים
                if (data == null || data.Length == 0)
                {
                    Console.WriteLine("Audio data is null or empty");
                    return "";
                }

                try
                {
                    // זיהוי סוג הקובץ לפי Magic Numbers
                    string mimeType = GetAudioMimeType(data);
                    
                    // המרה ל-Base64
                    string base64 = Convert.ToBase64String(data);
                    
                    // יצירת Data URL
                    return $"data:{mimeType};base64,{base64}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting audio data: {ex.Message}");
                    return "";
                }
            }
            
            /// <summary>
            /// מזהה את סוג קובץ האודיו לפי ה-Magic Numbers שלו
            /// Magic Numbers הם הבתים הראשונים בקובץ שמזהים את סוגו
            /// </summary>
            /// <param name="data">מערך הבתים של הקובץ</param>
            /// <returns>MIME type של הקובץ</returns>
            private string GetAudioMimeType(byte[] data)
            {
                // אם הקובץ קטן מדי, נניח שזה MP3
                if (data.Length < 12)
                {
                    return "audio/mpeg";
                }

                // בדיקה 1: MP3 עם תג ID3
                // ID3 = "ID3" באותיות ASCII
                // 0x49='I', 0x44='D', 0x33='3'
                if (data.Length >= 10 &&
                    data[0] == 0x49 && data[1] == 0x44 && data[2] == 0x33)
                {
                    return "audio/mpeg";
                }

                // בדיקה 2: MP3 ללא תג ID3
                // MP3 מתחיל ב-0xFF ואחריו 0xE0-0xFF
                // & 0xE0 בודק את 3 הביטים העליונים
                if (data.Length >= 2 &&
                    (data[0] == 0xFF && (data[1] & 0xE0) == 0xE0))
                {
                    return "audio/mpeg";
                }

                // בדיקה 3: WAV
                // WAV מתחיל ב-"RIFF" ואחר כך "WAVE"
                if (data.Length >= 12 &&
                    data[0] == 0x52 && data[1] == 0x49 && 
                    data[2] == 0x46 && data[3] == 0x46 &&
                    data[8] == 0x57 && data[9] == 0x41 && 
                    data[10] == 0x56 && data[11] == 0x45)
                {
                    return "audio/wav";
                }

                // בדיקה 4: AAC
                // AAC מתחיל ב-0xFF ואחריו 0xF0-0xFF
                if (data.Length >= 4 &&
                    data[0] == 0xFF && (data[1] & 0xF0) == 0xF0)
                {
                    return "audio/aac";
                }

                // ברירת מחדל: MP3
                return "audio/mpeg";
            }

            /// <summary>
            /// Constructor ריק
            /// </summary>
            public Song()
            {
            }
        }
    }
    ```

    ---

    ## DBL

    ### BaseDB.cs - מחלקת בסיס גנרית

    ```csharp
    using Models;
    using System.Collections.Generic;
    using System.Data.Common;

    namespace DBL
    {
        /// <summary>
        /// מחלקת בסיס גנרית לכל פעולות CRUD
        /// T = סוג המודל (User, Song, וכו')
        /// כל מחלקת DBL יורשת ממחלקה זו
        /// </summary>
        public abstract class BaseDB<T> : DB
        {
            /// <summary>
            /// מחזיר את שם הטבלה במסד הנתונים
            /// כל מחלקה יורשת חייבת לממש
            /// </summary>
            protected abstract string GetTableName();
            
            /// <summary>
            /// מחזיר את שם המפתח הראשי
            /// כל מחלקה יורשת חייבת לממש
            /// </summary>
            protected abstract string GetPrimaryKeyName();
            
            /// <summary>
            /// יוצר אובייקט מודל משורה במסד נתונים
            /// כל מחלקה יורשת חייבת לממש
            /// </summary>
            /// <param name="row">מערך של ערכים מהמסד נתונים</param>
            /// <returns>אובייקט מסוג T</returns>
            protected abstract Task<T> CreateModelAsync(object[] row);
            
            /// <summary>
            /// שליפת כל הרשומות מהטבלה
            /// </summary>
            /// <returns>רשימה של אובייקטים מסוג T</returns>
            protected async Task<List<T>> SelectAllAsync()
            {
                return await SelectAllAsync("", new Dictionary<string, object>());
            }

            /// <summary>
            /// שליפת רשומות עם תנאים
            /// </summary>
            /// <param name="parameters">Dictionary של תנאי WHERE</param>
            /// <returns>רשימה של אובייקטים מסוג T</returns>
            protected async Task<List<T>> SelectAllAsync(Dictionary<string, object> parameters)
            {
                return await SelectAllAsync("", parameters);
            }

            /// <summary>
            /// שליפת רשומות עם שאילתה מותאמת אישית
            /// </summary>
            /// <param name="query">שאילתת SQL</param>
            /// <param name="parameters">פרמטרים לשאילתה</param>
            /// <returns>רשימה של אובייקטים מסוג T</returns>
            protected async Task<List<T>> SelectAllAsync(string query, Dictionary<string, object> parameters)
            {
                List<object[]> list = await StingListSelectAllAsync(query, parameters);
                return await CreateListModelAsync(list);
            }

            /// <summary>
            /// הוספת רשומה חדשה
            /// </summary>
            /// <param name="keyAndValue">Dictionary של שדות וערכים</param>
            /// <returns>מספר השורות שהושפעו (1 = הצלחה)</returns>
            protected async Task<int> InsertAsync(Dictionary<string, object> keyAndValue)
            {
                string sqlCommand = PrepareInsertQueryWithParameters(keyAndValue);
                return await ExecNonQueryAsync(sqlCommand);
            }

            /// <summary>
            /// עדכון רשומה קיימת
            /// </summary>
            /// <param name="fieldsToUpdate">שדות לעדכון</param>
            /// <param name="whereConditions">תנאי WHERE</param>
            /// <returns>מספר השורות שהושפעו</returns>
            protected async Task<int> UpdateAsync(
                Dictionary<string, object> fieldsToUpdate, 
                Dictionary<string, object> whereConditions)
            {
                string where = PrepareWhereQueryWithParameters(whereConditions);
                string setClause = PrepareUpdateQueryWithParameters(fieldsToUpdate);
                
                if (string.IsNullOrEmpty(setClause))
                    return 0;

                string sqlCommand = $"UPDATE {GetTableName()} SET {setClause} {where}";
                return await ExecNonQueryAsync(sqlCommand);
            }

            /// <summary>
            /// מחיקת רשומה
            /// </summary>
            /// <param name="whereConditions">תנאי WHERE</param>
            /// <returns>מספר השורות שנמחקו</returns>
            protected async Task<int> DeleteAsync(Dictionary<string, object> whereConditions)
            {
                string where = PrepareWhereQueryWithParameters(whereConditions);
                string sqlCommand = $"DELETE FROM {GetTableName()} {where}";
                return await ExecNonQueryAsync(sqlCommand);
            }

            // שיטות עזר פרטיות...
            // (הקוד המלא כולל עוד שיטות עזר לבניית שאילתות)
        }
    }
    ```

    ---

    ### UserDB.cs - ניהול משתמשים

    ```csharp
    using Models;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    namespace DBL
    {
        /// <summary>
        /// מחלקה לניהול משתמשים במסד הנתונים
        /// כוללת פעולות הרשמה, התחברות, עדכון ומחיקה
        /// </summary>
        public class UserDB : BaseDB<User>
        {
            /// <summary>
            /// מחזיר את שם הטבלה
            /// </summary>
            protected override string GetTableName() => "users";
            
            /// <summary>
            /// מחזיר את שם המפתח הראשי
            /// </summary>
            protected override string GetPrimaryKeyName() => "userid";
            
            /// <summary>
            /// יוצר אובייקט User משורה במסד נתונים
            /// </summary>
            protected async override Task<User> CreateModelAsync(object[] row)
            {
                User u = new User();
                u.userid = int.Parse(row[0].ToString());
                u.username = row[1].ToString();
                u.password = row[2].ToString();
                u.email = row[3].ToString();
                u.profilepicture = row[4] as byte[];
                u.IsAdmin = int.Parse(row[5].ToString());
                u.ResetCode = row[6]?.ToString();
                return u;
            }

            /// <summary>
            /// הרשמת משתמש חדש למערכת
            /// כוללת בדיקות תקינות והצפנת סיסמה
            /// </summary>
            /// <param name="username">שם משתמש</param>
            /// <param name="password">סיסמה (תוצפן אוטומטית)</param>
            /// <param name="email">דוא"ל</param>
            /// <returns>Tuple: (הצלחה, הודעה)</returns>
            public async Task<(bool Success, string Message)> RegisterAsync(
                string username, string password, string email)
            {
                // בדיקה 1: שדות חובה
                if (string.IsNullOrWhiteSpace(username) || 
                    string.IsNullOrWhiteSpace(password) || 
                    string.IsNullOrWhiteSpace(email))
                    return (false, "All fields are required.");

                // בדיקה 2: פורמט דוא"ל
                if (!IsValidEmail(email))
                    return (false, "Invalid email format.");

                // בדיקה 3: שם משתמש קיים
                if (await UsernameExistsAsync(username))
                    return (false, "Username already taken.");

                // בדיקה 4: דוא"ל קיים
                if (await EmailExistsAsync(email))
                    return (false, "Email already registered.");

                // הצפנת סיסמה ב-SHA256
                string hashedPassword = HashPassword(password);

                // הוספה למסד נתונים
                Dictionary<string, object> values = new Dictionary<string, object>
                {
                    { "username", username },
                    { "password", hashedPassword },
                    { "email", email },
                    { "IsAdmin", 0 }  // משתמש רגיל
                };

                int rows = await InsertAsync(values);

                if (rows == 1)
                    return (true, "Registration successful.");

                return (false, "Registration failed due to unknown error.");
            }

            /// <summary>
            /// התחברות למערכת
            /// </summary>
            /// <param name="username">שם משתמש</param>
            /// <param name="password">סיסמה</param>
            /// <returns>אובייקט User אם הצליח, null אם נכשל</returns>
            public async Task<User> LoginAsync(string username, string password)
            {
                // הצפנת הסיסמה להשוואה
                string hashedPassword = HashPassword(password);

                // שאילתה עם פרמטרים (הגנה מפני SQL Injection)
                string sql = "SELECT * FROM users WHERE username=@username AND password=@password";
                var parameters = new Dictionary<string, object>
                {
                    { "username", username },
                    { "password", hashedPassword }
                };

                List<User> list = await SelectAllAsync(sql, parameters);
                
                // אם נמצא בדיוק משתמש אחד - הצלחה
                if (list.Count == 1)
                    return list[0];

                // אחרת - כישלון
                return null;
            }

            /// <summary>
            /// הצפנת סיסמה ב-SHA256
            /// </summary>
            /// <param name="password">סיסמה בטקסט פשוט</param>
            /// <returns>סיסמה מוצפנת ב-Base64</returns>
            private string HashPassword(string password)
            {
                using SHA256 sha = SHA256.Create();
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }

            /// <summary>
            /// בדיקה אם שם משתמש קיים
            /// </summary>
            private async Task<bool> UsernameExistsAsync(string username)
            {
                string sql = "SELECT * FROM users WHERE username=@username";
                List<User> list = await SelectAllAsync(sql, 
                    new Dictionary<string, object> { { "username", username } });
                return list.Count > 0;
            }

            /// <summary>
            /// בדיקה אם דוא"ל קיים
            /// </summary>
            private async Task<bool> EmailExistsAsync(string email)
            {
                string sql = "SELECT * FROM users WHERE email=@email";
                List<User> list = await SelectAllAsync(sql, 
                    new Dictionary<string, object> { { "email", email } });
                return list.Count > 0;
            }

            /// <summary>
            /// בדיקת תקינות דוא"ל באמצעות Regex
            /// </summary>
            private bool IsValidEmail(string email)
            {
                string pattern = @"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b";
                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            }

            /// <summary>
            /// שליפת כל המשתמשים (למנהלים)
            /// </summary>
            public async Task<List<User>> GetAllUsersAsync()
            {
                return await SelectAllAsync();
            }

            /// <summary>
            /// מחיקת משתמש
            /// </summary>
            public async Task<int> DeleteUserAsync(int userId)
            {
                Dictionary<string, object> filter = new Dictionary<string, object>();
                filter.Add("userid", userId);
                return await DeleteAsync(filter);
            }
        }
    }
    ```

    ---

    **הערה:** זהו חלק מהקוד. הקוד המלא כולל עוד מחלקות:
    - SongDB.cs
    - PlaylistDB.cs
    - GenreDB.cs
    - RatingDB.cs
    - Controllers (SongsController, UsersController, וכו')
    - Blazor Pages (Login.razor, Upload.razor, וכו')

    **סיום נספח ב'**

    ---

    ## הערה חשובה

    קוד המקור המלא זמין בתיקיות הפרויקט:
    - `Models/` - כל המודלים
    - `DBL/` - כל מחלקות הגישה לנתונים
    - `AruroaAPI/Controllers/` - כל ה-Controllers
    - `AruroaBlazor/Components/Pages/` - כל דפי Blazor

    **לצפייה בקוד המלא, פתח את הקבצים בתיקיות אלו.**
