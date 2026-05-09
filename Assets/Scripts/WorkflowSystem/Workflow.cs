using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Workflow
{
    private List<IStep> _steps = new List<IStep>();
    private IStep _currentStep = null;

    public event Action OnComplete;
    private ILogService _logService;
    private IAlertService _alertService;
    private IEventService _eventService;

    public Workflow(List<IStep> workflowSteps)
    {
        this._steps = workflowSteps;
        _logService = AppContainer.Get<ILogService>();
        _alertService = AppContainer.Get<IAlertService>();
        _eventService = AppContainer.Get<IEventService>();
    }

    public void Begin()
    {
        if (this._currentStep != null)
            return;
        if (this._steps.Count == 0)
            return;
        this.ActivateStep(this._steps[0]);
    }

    private void ActivateStep(IStep step)
    {
        if (step == null) return;

        this._currentStep = step;

        this._currentStep.Activate();
        this._currentStep.OnCompleted += StepCompleted;
    }

    private void DeactivateCurrentStep()
    {
        if (this._currentStep == null)
            return;
        this._currentStep.OnCompleted -= StepCompleted;
        this._currentStep.Deactivate();
        this._currentStep = null;
    }

    private void StepCompleted()
    {
        var indexOfCurrentStep = this._steps.IndexOf(this._currentStep);

        if (indexOfCurrentStep == -1)
        {
            _logService.Add<Workflow>($"No se encuentra el step {this._currentStep.Name}");
            return;
        }

        this.DeactivateCurrentStep();
        _eventService.Publish(new OnAlertMessageReceived(null, null)); // cierra el alert actual

        if (indexOfCurrentStep == this._steps.Count - 1)
        {
            CompleteWorkflow();
            return;
        }

        var nextStep = this._steps[indexOfCurrentStep + 1];
        ActivateStepAfterDelay(nextStep);
    }

    private async void ActivateStepAfterDelay(IStep nextStep)
    {
        await Task.Delay(TimeSpan.FromSeconds(2f));
        this.ActivateStep(nextStep);
    }

    private async void CompleteWorkflow()
    {
        await Task.Delay(TimeSpan.FromSeconds(2f));
        _alertService.Show("¡Has completado el tutorial!", "¡Felicidades!");
        await Task.Delay(TimeSpan.FromSeconds(3f)); // tiempo para leer el mensaje
        _eventService.Publish(new OnAlertMessageReceived(null, null)); // cierra el alert
        OnComplete?.Invoke();
    }
}
