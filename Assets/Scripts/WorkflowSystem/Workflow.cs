using System;
using System.Collections.Generic;

public class Workflow
{
    private List<IStep> _steps = new List<IStep>();
    private IStep _currentStep = null;

    public event Action OnComplete;
    private ILogService _logService;
    private IAlertService _alertService;

    public Workflow(List<IStep> workflowSteps)
    {
        this._steps = workflowSteps;
        _logService = AppContainer.Get<ILogService>();
        _alertService = AppContainer.Get<IAlertService>();
    }

    public void Begin()
    {
        if (this._currentStep != null)
            return;
        if(this._steps.Count == 0)
            return;
        this.ActivateStep(this._steps[0]);
    }

    private void ActivateStep(IStep step)
    {
        if(step == null) return;

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

        if(indexOfCurrentStep == -1)
        {
            _logService.Add<Workflow>($"No se encuentra el step {this._currentStep.Name}");
            return;
        }

        if(indexOfCurrentStep == this._steps.Count - 1)
        {
            CompleteWorkflow();
            OnComplete?.Invoke();
            this.DeactivateCurrentStep();
            return;
        }

        var nextStep = this._steps[indexOfCurrentStep + 1];

        this.DeactivateCurrentStep();
        this.ActivateStep(nextStep);
    }

    private void CompleteWorkflow()
    {
        //_logService.Add<Workflow>($"Workflow completo");
        _alertService.Show("¡Has completado el tutorial!", "¡Felicidades!");
    }
}
