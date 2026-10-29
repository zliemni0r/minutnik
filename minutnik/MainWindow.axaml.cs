using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace minutnik;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private int _remainingSeconds;
    private int _totalPhaseSeconds;
    private int _completedCycles;
    private bool _isWorkMode = true;
    private bool _isRunning;

    private readonly IBrush _workBadgeBackground = Brush.Parse("#1E3A8A");
    private readonly IBrush _workBadgeBorder = Brush.Parse("#3B82F6");
    private readonly IBrush _workBadgeForeground = Brush.Parse("#93C5FD");
    private readonly IBrush _workDotFill = Brush.Parse("#60A5FA");
    private readonly IBrush _workProgressFill = Brush.Parse("#3B82F6");

    private readonly IBrush _breakBadgeBackground = Brush.Parse("#064E3B");
    private readonly IBrush _breakBadgeBorder = Brush.Parse("#10B981");
    private readonly IBrush _breakBadgeForeground = Brush.Parse("#6EE7B7");
    private readonly IBrush _breakDotFill = Brush.Parse("#34D399");
    private readonly IBrush _breakProgressFill = Brush.Parse("#10B981");

    private readonly IBrush _startButtonBrush = Brush.Parse("#2563EB");
    private readonly IBrush _pauseButtonBrush = Brush.Parse("#D97706");

    public MainWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTimerTick;

        StartPauseButton.Click += OnStartPauseClicked;
        ResetButton.Click += OnResetClicked;
        WorkDurationNumericUpDown.ValueChanged += OnWorkDurationChanged;
        BreakDurationNumericUpDown.ValueChanged += OnBreakDurationChanged;

        InitializeTimerState();
    }

    private void InitializeTimerState()
    {
        _isWorkMode = true;
        _completedCycles = 0;
        _isRunning = false;

        int workMinutes = GetWorkDurationMinutes();
        _totalPhaseSeconds = workMinutes * 60;
        _remainingSeconds = _totalPhaseSeconds;

        UpdateDisplay();
    }

    private int GetWorkDurationMinutes()
    {
        if (WorkDurationNumericUpDown?.Value is decimal value && value >= 1)
        {
            return (int)value;
        }
        return 25;
    }

    private int GetBreakDurationMinutes()
    {
        if (BreakDurationNumericUpDown?.Value is decimal value && value >= 1)
        {
            return (int)value;
        }
        return 5;
    }

    private void OnStartPauseClicked(object? sender, RoutedEventArgs e)
    {
        if (!_isRunning)
        {
            _isRunning = true;
            _timer.Start();
        }
        else
        {
            _isRunning = false;
            _timer.Stop();
        }

        UpdateDisplay();
    }

    private void OnResetClicked(object? sender, RoutedEventArgs e)
    {
        _timer.Stop();
        InitializeTimerState();
    }

    private void OnWorkDurationChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (!_isRunning && _isWorkMode)
        {
            int workMinutes = GetWorkDurationMinutes();
            _totalPhaseSeconds = workMinutes * 60;
            _remainingSeconds = _totalPhaseSeconds;
            UpdateDisplay();
        }
    }

    private void OnBreakDurationChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (!_isRunning && !_isWorkMode)
        {
            int breakMinutes = GetBreakDurationMinutes();
            _totalPhaseSeconds = breakMinutes * 60;
            _remainingSeconds = _totalPhaseSeconds;
            UpdateDisplay();
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _remainingSeconds--;

        if (_remainingSeconds <= 0)
        {
            if (_isWorkMode)
            {
                // Work session completed -> Break starts automatically
                _isWorkMode = false;
                int breakMinutes = GetBreakDurationMinutes();
                _totalPhaseSeconds = breakMinutes * 60;
                _remainingSeconds = _totalPhaseSeconds;
            }
            else
            {
                // Break session completed -> Increment completed cycles and switch back to work session
                _completedCycles++;
                _isWorkMode = true;
                int workMinutes = GetWorkDurationMinutes();
                _totalPhaseSeconds = workMinutes * 60;
                _remainingSeconds = _totalPhaseSeconds;
            }
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        int displaySeconds = Math.Max(0, _remainingSeconds);
        int minutes = displaySeconds / 60;
        int seconds = displaySeconds % 60;

        TimerTextBlock.Text = $"{minutes:D2}:{seconds:D2}";
        CycleCountTextBlock.Text = _completedCycles.ToString();

        if (_isWorkMode)
        {
            StatusTextBlock.Text = "TRYB PRACY";
            StatusTextBlock.Foreground = _workBadgeForeground;
            StatusBadgeBorder.Background = _workBadgeBackground;
            StatusBadgeBorder.BorderBrush = _workBadgeBorder;
            StatusDot.Fill = _workDotFill;
            PhaseProgressBar.Foreground = _workProgressFill;
        }
        else
        {
            StatusTextBlock.Text = "PRZERWA";
            StatusTextBlock.Foreground = _breakBadgeForeground;
            StatusBadgeBorder.Background = _breakBadgeBackground;
            StatusBadgeBorder.BorderBrush = _breakBadgeBorder;
            StatusDot.Fill = _breakDotFill;
            PhaseProgressBar.Foreground = _breakProgressFill;
        }

        if (_isRunning)
        {
            StartPauseButton.Content = "Pauza";
            StartPauseButton.Background = _pauseButtonBrush;
        }
        else
        {
            StartPauseButton.Content = "Start";
            StartPauseButton.Background = _startButtonBrush;
        }

        if (_totalPhaseSeconds > 0)
        {
            double progress = (double)(_totalPhaseSeconds - _remainingSeconds) / _totalPhaseSeconds * 100.0;
            PhaseProgressBar.Value = Math.Clamp(progress, 0, 100);
        }
        else
        {
            PhaseProgressBar.Value = 0;
        }
    }
}