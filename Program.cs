using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading;
using System.Security.AccessControl;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

public class MainForm : Form
{
    private static readonly string LabPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SimuladorRamsonware");
    private static readonly string BackupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SimuladorBackup");
    private static Dictionary<string, string> initialHashes = new Dictionary<string, string>();

    // Controles de la interfaz gráfica
    private TextBox txtLog;
    private Button btnMod1, btnMod2, btnMod3, btnMod4, btnMod5, btnSetup;

    public MainForm()
    {
        Text = "SOC Defense Lab - Simulador Antirransomware";
        Width = 860;
        Height = 640;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(13, 17, 23); // Tema oscuro moderno SOC

        InitializeComponent();
        
        // Ejecución lógica inicial intacta
        Log("=== LABORATORIO DE SIMULACIÓN Y DETECCIÓN DE RANSOMWARE (C#) ===\n");
        SetupLab();
    }

    private void InitializeComponent()
    {
        // 1. Panel de Cabecera Superior
        var panelHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 65,
            BackColor = Color.FromArgb(22, 27, 34),
            Padding = new Padding(15)
        };

        var lblTitle = new Label
        {
            Text = "🛡️ Security Operations Center - Menú de Escenarios",
            ForeColor = Color.FromArgb(240, 246, 252),
            Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(15, 18)
        };
        panelHeader.Controls.Add(lblTitle);

        // 2. Panel de Botones de Acción (Reemplazo visual del menú por consola)
        var panelButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 85,
            Padding = new Padding(10, 12, 10, 10),
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.FromArgb(13, 17, 23)
        };

        btnMod1 = CreateButton("1. Modif. Normal", Color.FromArgb(35, 134, 54), (s, e) => { SimularModificacionNormal(); });
        btnMod2 = CreateButton("2. Ataque Masivo", Color.FromArgb(218, 54, 51), (s, e) => { SimularAtaqueMasivo(); });
        btnMod3 = CreateButton("3. Eliminar Arch.", Color.FromArgb(187, 128, 9), (s, e) => { SimularEliminacion(); });
        btnMod4 = CreateButton("4. Ejecutar Detector", Color.FromArgb(137, 87, 229), (s, e) => { EjecutarDetector(3, 5); });
        btnMod5 = CreateButton("5. Restaurar Backup", Color.FromArgb(8, 153, 129), (s, e) => { RestaurarBackup(); });
        btnSetup = CreateButton("🔄 Reiniciar Lab", Color.FromArgb(31, 111, 235), (s, e) => { SetupLab(); });

        panelButtons.Controls.Add(btnMod1);
        panelButtons.Controls.Add(btnMod2);
        panelButtons.Controls.Add(btnMod3);
        panelButtons.Controls.Add(btnMod4);
        panelButtons.Controls.Add(btnMod5);
        panelButtons.Controls.Add(btnSetup);

        // 3. Consola de Texto / Registro (Log)
        var panelLogContainer = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 0, 12, 12),
            BackColor = Color.FromArgb(13, 17, 23)
        };

        txtLog = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(22, 27, 34),
            ForeColor = Color.FromArgb(121, 192, 255),
            BorderStyle = BorderStyle.None,
            Font = new Font("Consolas", 10.5F, FontStyle.Regular)
        };

        panelLogContainer.Controls.Add(txtLog);

        Controls.Add(panelLogContainer);
        Controls.Add(panelButtons);
        Controls.Add(panelHeader);
    }

    private Button CreateButton(string text, Color baseColor, EventHandler onClick)
    {
        var btn = new Button
        {
            Text = text,
            Width = 125,
            Height = 52,
            BackColor = baseColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(4)
        };
        btn.FlatAppearance.BorderSize = 0;
        
        btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(baseColor, 0.15f);
        btn.MouseLeave += (s, e) => btn.BackColor = baseColor;
        
        btn.Click += onClick;
        return btn;
    }

    private static void Log(string message)
    {
        // Redirige o imprime mensajes manteniendo la lógica intacta
        // Si hay una instancia activa, escribe en la caja de texto gráfica
        if (Application.OpenForms.Count > 0 && Application.OpenForms[0] is MainForm mainForm)
        {
            mainForm.AppendLog(message);
        }
    }

    private void AppendLog(string message)
    {
        if (txtLog.InvokeRequired)
        {
            txtLog.Invoke(new Action<string>(AppendLog), message);
        }
        else
        {
            txtLog.AppendText(message + Environment.NewLine);
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }
    }

    // ==========================================
    // LÓGICA ORIGINAL INTACTA
    // ==========================================

    private static void RestablecerPermisosConIcacls(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "icacls.exe",
                    Arguments = $"\"{path}\" /reset /T /C /Q",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var process = Process.Start(psi))
                {
                    process?.WaitForExit();
                }
            }
        }
        catch (Exception ex)
        {
            Log($"[!] Error al restablecer permisos con icacls: {ex.Message}");
        }
    }

    private static void SetupLab()
    {
        if (Directory.Exists(LabPath))
        {
            RestablecerPermisosConIcacls(LabPath);
            Thread.Sleep(100);
            try
            {
                Directory.Delete(LabPath, true);
            }
            catch (Exception ex)
            {
                Log($"[!] Error al eliminar directorio de laboratorio anterior: {ex.Message}");
            }
        }
        
        if (Directory.Exists(BackupPath))
        {
            try
            {
                Directory.Delete(BackupPath, true);
            }
            catch { }
        }

        Directory.CreateDirectory(LabPath);
        Directory.CreateDirectory(BackupPath);

        Log("[*] Creando archivos de prueba ficticios...");
        for (int i = 1; i <= 5; i++)
        {
            string filePath = Path.Combine(LabPath, $"documento_secreto_{i}.txt");
            File.WriteAllText(filePath, $"Este es el contenido original del archivo confidencial número {i}.");
        }

        foreach (var file in Directory.GetFiles(LabPath))
        {
            string fileName = Path.GetFileName(file);
            File.Copy(file, Path.Combine(BackupPath, fileName), true);
        }
        Log("[+] Copia de seguridad inicial creada exitosamente en /SimuladorBackup.");

        // Calcular Hashes Iniciales
        initialHashes.Clear();
        foreach (var file in Directory.GetFiles(LabPath))
        {
            initialHashes[file] = CalcularSha256(file);
        }
        Log($"[+] Hashes SHA-256 calculados para {initialHashes.Count} archivos.\n");
    }

    private static string CalcularSha256(string filePath)
    {
        try
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    private static void SimularModificacionNormal()
    {
        string[] files = Directory.GetFiles(LabPath);
        if (files.Length == 0)
        {
            Log("[!] No hay archivos en el laboratorio.");
            return;
        }

        string fileToEdit = files[0];
        File.AppendAllText(fileToEdit, "\n[Modificación legítima realizada por el usuario]");
        Log($"[SIMULACIÓN] Archivo modificado legítimamente: {Path.GetFileName(fileToEdit)}");
    }

    private static void SimularAtaqueMasivo()
    {
        Log("[SIMULACIÓN] ¡Simulando comportamiento de ransomware (cifrado/alteración masiva rápida)...");
        string[] files = Directory.GetFiles(LabPath);
        
        foreach (var file in files)
        {
            File.WriteAllText(file, "BLOQUEO_RANSOMWARE_ENCRYPTED_CONTENT");
            Log($" -> Alterado maliciosamente: {Path.GetFileName(file)}");
            Thread.Sleep(200); 
        }
        Log("[!] ¡Ataque masivo simulado completado!");
    }

    private static void SimularEliminacion()
    {
        string[] files = Directory.GetFiles(LabPath);
        if (files.Length > 0)
        {
            File.Delete(files[0]);
            Log($"[SIMULACIÓN] Archivo eliminado: {Path.GetFileName(files[0])}");
        }
        else
        {
            Log("[!] No hay archivos para eliminar.");
            return;
        }
    }

    private static void EjecutarDetector(int limiteModificaciones, int ventanaSegundos)
    {
        Log($"\n[DETECTOR] Iniciando monitoreo de integridad (Límite: {limiteModificaciones} cambios)...");
        
        List<string> archivosAfectados = new List<string>();
        string fechaHoraActual = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string tipoCambioDetectado = "Modificación de Integridad / Cifrado";

        foreach (var kvp in initialHashes)
        {
            string filePath = kvp.Key;
            string originalHash = kvp.Value;
            string fileName = Path.GetFileName(filePath);

            if (!File.Exists(filePath))
            {
                Log($"[ALERTA MEDIA] [{fechaHoraActual}] Archivo ELIMINADO detectado: {fileName}");
                archivosAfectados.Add($"{fileName} (Eliminado)");
                tipoCambioDetectado = "Eliminación de Archivos";
            }
            else
            {
                string currentHash = CalcularSha256(filePath);
                if (currentHash != originalHash)
                {
                    Log($"[ALERTA ALTA] [{fechaHoraActual}] Modificación detectada en: {fileName}");
                    Log($"    -> Hash Original : {originalHash.Substring(0, 10)}...");
                    Log($"    -> Hash Actual   : {currentHash.Substring(0, 10)}...");
                    archivosAfectados.Add($"{fileName} (Modificado / Cifrado)");
                }
            }
        }

        if (archivosAfectados.Count >= limiteModificaciones)
        {
            Log("\n========================================================");
            Log("🚨 ¡ALERTA CRÍTICA DE RANSOMWARE DETECTADO! 🚨");
            Log($"Se detectaron {archivosAfectados.Count} cambios anómalos de integridad.");
            Log("Acción recomendada: Carpeta bloqueada automáticamente.");
            Log("========================================================");

            BloquearCarpetaYMostrarAlerta(fechaHoraActual, archivosAfectados, tipoCambioDetectado);
        }
        else
        {
            Log("\n[+] Estado del sistema: Normal. Actividad dentro de los parámetros seguros.");
        }
    }

    private static void BloquearCarpetaYMostrarAlerta(string fecha, List<string> archivosAfectados, string tipoCambio)
    {
        try
        {
            DirectoryInfo dInfo = new DirectoryInfo(LabPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            dSecurity.AddAccessRule(new FileSystemAccessRule(
                Environment.UserName, 
                FileSystemRights.FullControl, 
                AccessControlType.Deny));
            dInfo.SetAccessControl(dSecurity);
        }
        catch (Exception ex)
        {
            Log($"[!] Nota sobre permisos de bloqueo: {ex.Message}");
        }

        string detalle = string.Join(Environment.NewLine, archivosAfectados);
        string mensaje =
            "¡ACCESO BLOQUEADO POR SEGURIDAD!\n\n" +
            "Se detectó actividad anómala compatible con un ataque de ransomware.\n\n" +
            $"📅 Fecha y Hora: {fecha}\n" +
            $"⚡ Tipo de Cambio: {tipoCambio}\n\n" +
            $"Archivos afectados:\n{detalle}\n\n" +
            "La carpeta ha sido bloqueada para evitar mayor destrucción.\n" +
            "Utiliza la opción de restaurar backup (Opción 5) para recuperar el acceso.";

        MessageBox.Show(
            mensaje,
            "🚨 Alerta Crítica - Carpeta Cifrada Bloqueada",
            MessageBoxButtons.OK,
            MessageBoxIcon.Stop
        );
    }

    private static void RestaurarBackup()
    {
        Log("[RECUPERACIÓN] Restaurando permisos y archivos desde la copia de seguridad...");
        
        if (!Directory.Exists(BackupPath))
        {
            Log("[!] No se encontró la carpeta de respaldo.");
            return;
        }

        if (Directory.Exists(LabPath))
        {
            RestablecerPermisosConIcacls(LabPath);
            Thread.Sleep(100);
            try
            {
                Directory.Delete(LabPath, true);
            }
            catch (Exception ex)
            {
                Log($"[!] Advertencia al eliminar directorio: {ex.Message}");
            }
        }

        Directory.CreateDirectory(LabPath);
        foreach (var file in Directory.GetFiles(BackupPath))
        {
            string fileName = Path.GetFileName(file);
            File.Copy(file, Path.Combine(LabPath, fileName), true);
        }

        Log("[+] ¡Permisos restablecidos y archivos restaurados exitosamente a su estado original!");
    }
}